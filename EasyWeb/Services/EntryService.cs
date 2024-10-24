﻿using System.Data;
using System.Security.Cryptography;
using System.Web;
using EasyWeb.Data;
using EasyWeb.Models;
using NewLife;
using NewLife.Caching;
using NewLife.Http;
using NewLife.Log;
using NewLife.Security;

namespace EasyWeb.Services;

public class EntryService
{
    private const Int32 CacheTime = 10;
    private readonly ICacheProvider _cacheProvider;
    private readonly ITracer _tracer;

    public EntryService(ICacheProvider cacheProvider, ITracer tracer)
    {
        _cacheProvider = cacheProvider;
        _tracer = tracer;
    }

    /// <summary>获取默认存储</summary>
    /// <returns></returns>
    public FileStorage GetDefaultStorage()
    {
        return _cacheProvider.Cache.GetOrAdd($"DefaultStorage",
            k => FileStorage.FindAllWithCache().FirstOrDefault(e => e.Enable), CacheTime);
    }

    public IList<FileEntry> GetEntries(Int32 storageId, Int32 parentId)
    {
        if (storageId == 0) storageId = GetDefaultStorage()?.Id ?? 0;

        return _cacheProvider.Cache.GetOrAdd($"Entries:{storageId}:{parentId}",
            k => FileEntry.Search(storageId, parentId, true), CacheTime);
    }

    public FileEntry GetEntry(Int32 storageId, String path)
    {
        if (path.IsNullOrEmpty()) return null;

        if (storageId == 0) storageId = GetDefaultStorage()?.Id ?? 0;

        return _cacheProvider.Cache.GetOrAdd($"Entry:{storageId}:{path}",
            k => FileEntry.FindByStorageIdAndPath(storageId, path), CacheTime);
    }

    public FileModel BuildModel(FileEntry entry, Boolean useVip, Boolean isHttps)
    {
        if (entry == null) return null;

        // 构建下载Url
        var url = entry.Path;
        if (url.IsNullOrEmpty()) url = $"{entry.Parent?.Path}/{entry.Name}";
        url = url?.Split('/').Select(e => HttpUtility.UrlEncode(e)).Join("/").EnsureStart("/");

        if (useVip && !entry.IsDirectory) url = GetAuthUrl(entry.Storage, url);

        // 如果是Https请求，则生成配套的https地址
        if (!entry.IsDirectory && isHttps)
        {
            if (url.StartsWithIgnoreCase("http://"))
                url = "https://" + url[7..];
        }

        return new FileModel
        {
            Name = entry.Name,
            Path = entry.Path,
            ParentPath = entry.Parent?.Path,
            Title = entry.Title,
            Size = entry.Size,
            LastWrite = entry.LastWrite,
            IsDirectory = entry.IsDirectory,
            Hash = entry.Hash,
            Url = url,
        };
    }

    public String GetAuthUrl(FileStorage storage, String url)
    {
        if (storage == null || url.IsNullOrEmpty()) return url;

        if (!storage.VipKey.IsNullOrEmpty())
        {
            // http://DomainName/Filename?auth_key={<timestamp>-rand-uid-<md5hash>}
            var path = url;
            var p = path.IndexOf("://");
            if (p > 0)
            {
                var p2 = path.IndexOf('/', p + 3);
                if (p2 > 0)
                    path = path[p2..];
                else
                    path = "/";
            }
            else if (path[0] != '/')
            {
                path = "/" + path;
            }

            var time = DateTime.UtcNow.ToInt();
            var rand = Rand.Next(100_000, 1_000_000);
            var hash = $"{path}-{time}-{rand}-0-{storage.VipKey}".MD5().ToLower();
            var key = $"{time}-{rand}-0-{hash}";

            url += url.Contains("?") ? "&" : "?";
            url += "auth_key=" + key;
        }

        if (!storage.VipUrl.IsNullOrEmpty())
        {
            //var uri = new UriInfo(url);
            //var uri2 = new UriInfo(storage.VipUrl)
            //{
            //    PathAndQuery = uri.PathAndQuery
            //};
            //url = uri.ToString();

            var p = url.IndexOf("://");
            if (p > 0)
            {
                var p2 = url.IndexOf('/', p + 3);
                if (p2 > 0)
                    url = url[p2..];
                else
                    url = "/";
            }

            url = storage.VipUrl.TrimEnd("/") + url;
        }

        return url;
    }

    /// <summary>获取文件，增加访问量。可能指向真实文件</summary>
    /// <param name="storageId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public (FileEntry, FileEntry link) RetrieveFile(Int32 storageId, String path)
    {
        var entry = GetEntry(storageId, path);
        if (entry == null || !entry.Enable) return (null, null);

        // 增加浏览数
        entry.Times++;
        entry.LastDownload = DateTime.Now;
        entry.SaveAsync(5_000);

        // 可能是链接文件，使用目标文件
        FileEntry link = null;
        if (!entry.LinkTarget.IsNullOrEmpty())
        {
            link = GetLink(entry);
            if (link == null || !link.Enable) return (null, null);

            // 更新连接文件信息到当前实体
            if (entry.Title.IsNullOrEmpty()) entry.Title = link.Title;
            if (entry.LastWrite < link.LastWrite) entry.LastWrite = link.LastWrite;
            entry.Size = link.Size;
            entry.Hash = link.Hash;

            link.Times++;
            link.LastDownload = DateTime.Now;
            link.SaveAsync(5_000);
        }

        return (entry, link);
    }

    public Boolean CheckHash(FileEntry entry, FileInfo fi)
    {
        // 校验哈希信息
        if (!entry.Hash.IsNullOrEmpty())
        {
            XTrace.WriteLine("校验文件哈希：{0} {1}", fi.FullName, entry.Hash);

            var hash = "";
            if (entry.Hash.Length <= 32)
                hash = fi.MD5().ToHex();
            else
            {
                using var fs = fi.OpenRead();
                hash = SHA512.Create().ComputeHash(fs).ToHex();
            }

            if (!hash.EqualIgnoreCase(entry.Hash))
            {
                XTrace.WriteLine("文件哈希不一致 {0} {1}", hash, entry.Hash);
                return false;
            }
        }

        return true;
    }

    public async Task<Boolean> DownloadAsync(FileEntry entry, String path)
    {
        var url = entry.RawUrl;
        if (url.IsNullOrEmpty()) return false;

        using var span = _tracer?.NewSpan(nameof(DownloadAsync), new { entry.Name, path, url });

        XTrace.WriteLine("文件不存在，准备下载 {0} => {1}", url, path);

        var client = new HttpClient();
        await client.DownloadFileAsync(url, path);

        if (!File.Exists(path)) return false;

        var fi = path.AsFile();
        XTrace.WriteLine("下载完成，大小：{0}", fi.Length.ToGMK());

        // 校验哈希信息
        if (!CheckHash(entry, fi))
        {
            _tracer?.NewError("DeleteFile-HashError", $"{entry.Path} {fi.FullName} {entry.Hash}");
            fi.Delete();
            return false;
        }

        // 缓存下载文件，不是简单更新最后写入时间
        if (entry.LastWrite.Year < 2000)
            entry.LastWrite = DateTime.Now;

        entry.Size = fi.Length;
        entry.LastAccess = DateTime.Now;
        entry.Update();

        var parent = entry.Parent;
        if (parent != null)
        {
            parent.Size = FileEntry.FindAllByParentId(parent.Id).Sum(e => e.Size);
            parent.Update();
        }

        return true;
    }

    /// <summary>获取目标链接文件</summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public FileEntry GetLink(FileEntry entry)
    {
        var link = entry.LinkTarget;
        if (link.IsNullOrEmpty()) return null;

        // 如果没有*模糊匹配，则直接查找
        if (!link.Contains('*'))
            return GetEntry(entry.StorageId, link);

        // 逐层匹配，可能有多级目录带有*
        var ss = link.Split('/');
        if (ss.Length == 0) return null;

        return MatchLink(entry.StorageId, 0, ss);

        //// 带有*的模糊匹配，先截断路径为目录和文件，*只能在文件名中
        //var p = link.LastIndexOf('/');
        //if (p < 0)
        //    throw new ArgumentOutOfRangeException(nameof(entry.LinkTarget), "链接目标不合法 " + entry.LinkTarget);

        //var dir = link[..p];
        //var file = link[(p + 1)..];
        //if (dir.Contains('*'))
        //    throw new ArgumentOutOfRangeException(nameof(entry.LinkTarget), "链接目标不合法 " + entry.LinkTarget);

        //// 查找目录
        //var parent = GetEntry(entry.StorageId, dir);
        //if (parent == null) return null;

        //var childs = GetEntries(entry.StorageId, parent.Id);
        //childs = childs.Where(e => file.IsMatch(e.Name)).ToList();
        //if (childs.Count == 0) return null;

        //// 返回最新的文件
        //return childs.OrderByDescending(e => e.LastWrite).FirstOrDefault();
    }

    private FileEntry MatchLink(Int32 storageId, Int32 parentId, String[] matchs)
    {
        return MatchLinks(storageId, parentId, matchs)
            .OrderByDescending(e => e.Version)
            .ThenBy(e => e.Name.Length)
            .FirstOrDefault();
    }

    private IList<FileEntry> MatchLinks(Int32 storageId, Int32 parentId, String[] matchs)
    {
        var m = matchs[0];
        var flag = true;
        if (m[0] == '!')
        {
            flag = false;
            m = m[1..];
        }

        var childs = GetEntries(storageId, parentId);

        // 最后一级找文件
        if (matchs.Length == 1)
        {
            if (flag)
                childs = childs.Where(e => !e.IsDirectory && m.IsMatch(e.Name)).ToList();
            else
                childs = childs.Where(e => !e.IsDirectory && !m.IsMatch(e.Name)).ToList();

            return childs;
        }
        else
        {
            if (flag)
                childs = childs.Where(e => e.IsDirectory && m.IsMatch(e.Name)).ToList();
            else
                childs = childs.Where(e => e.IsDirectory && !m.IsMatch(e.Name)).ToList();
            if (childs.Count == 0) return null;

            // 递归查找子目录，找到最新的文件
            var links = new List<FileEntry>();
            foreach (var item in childs)
            {
                var rs = MatchLinks(storageId, item.Id, matchs[1..]);
                if (rs != null) links.AddRange(rs);
            }

            //var fe = links.OrderByDescending(e => e.Version).FirstOrDefault();
            //return fe;
            if (links.Count > 0)
            {
                var max = links.Max(e => e.Version);
                links = links.Where(e => e.Version == max).ToList();
            }
            return links;
        }
    }

    public void FixVersionAndTag(FileEntry entry)
    {
        var name = entry.Name;
        if (name.IsNullOrEmpty() || !Version.TryParse(name, out _) && !name.Contains("net") && !name.Contains("runtime") && !name.Contains("-rc") && !name.Contains("-preview")) return;

        name = name.TrimEnd(".tar.gz", ".zip", ".exe", ".pkg");

        var ver = GetVersion(name);
        if (!ver.IsNullOrEmpty())
        {
            if (Version.TryParse(ver, out var v))
            {
                var n = 0;
                if (v.Major > 0) n += v.Major * 100_00_00;
                if (v.Minor > 0) n += v.Minor * 100_00;
                if (v.Build > 0) n += v.Build * 100;
                if (v.Revision > 0) n += v.Revision;

                entry.Version = n;
            }
            entry.Tag = name.Split('-').Where(e => e != ver && !e.Contains("rc") && !e.Contains("preview")).Join("-");
        }
    }

    String GetVersion(String name)
    {
        // aspnetcore-runtime-composite-9.0.0-preview.4.24267.6-linux-musl-arm64.tar.gz

        if (name.Contains('-'))
        {
            var p = name.LastIndexOf("-win");
            if (p < 0) p = name.LastIndexOf("-linux");
            if (p < 0) p = name.LastIndexOf("-osx");
            if (p < 0)
            {
                // 9.0.0-preview.5
                p = name.LastIndexOf("-preview");
                if (p > 0) return name.Replace("-preview", null);

                // 9.0.0-rc.2 为区分于预览版，尾部版本号补0
                p = name.LastIndexOf("-rc");
                if (p > 0) return name.Replace("-rc", null) + "0";

                return null;
            }

            name = name[..p];
        }

        var ss = name.Split('-');

        for (var i = ss.Length - 1; i >= 0; i--)
        {
            if (Version.TryParse(ss[i], out var ver) && ver.Major > 0) return ss[i];
        }

        return null;
    }

    public FileInfo GetFile(FileEntry entry)
    {
        var path = entry.FullName;
        if (entry.Storage != null) path = entry.Storage.HomeDirectory.CombinePath(path);

        path = path.GetFullPath();

        return path.AsFile();
    }

    public DirectoryInfo GetDirectory(FileEntry entry)
    {
        var path = entry.FullName;
        if (entry.Storage != null) path = entry.Storage.HomeDirectory.CombinePath(path);

        path = path.GetFullPath();

        return path.AsDirectory();
    }

    /// <summary>清理无效条目（含子目录）的文件，含禁用和原始跳转</summary>
    /// <param name="entry"></param>
    /// <param name="force">是否强迫删除，主要用于子目录递归</param>
    /// <returns></returns>
    public Int32 ClearFiles(FileEntry entry, Boolean force)
    {
        var rs = 0;
        if (entry.IsDirectory)
        {
            force |= !entry.Enable || entry.RedirectMode == RedirectModes.Redirect;

            var childs = FileEntry.FindAllByParentId(entry.Id);
            foreach (var item in childs)
            {
                rs += ClearFiles(item, force);
            }

            entry.Size = childs.Sum(e => e.Size);
            entry.Update();

            // 删除空目录
            var di = GetDirectory(entry);
            if (di.Exists && !di.GetFiles().Any()) di.Delete(true);
        }
        else if (force || !entry.Enable || entry.RedirectMode == RedirectModes.Redirect)
        {
            var fi = GetFile(entry);
            if (fi.Exists)
            {
                using var span = _tracer?.NewSpan("DeleteFile", new { entry.Name, entry.Path, entry.FullName, entry.Enable, entry.RedirectMode });

                fi.Delete();

                rs++;
            }

            entry.Size = 0;
            entry.Update();
        }

        return rs;
    }

    public Boolean ValidLimit(FileEntry entry, String ip, Int32 limitCycle = 600, Int64 maxSize = 100 * 1024 * 1024)
    {
        // 时间因子，今天总秒数除以周期
        var now = DateTime.Now;
        var sec = (Int32)(now - now.Date).TotalSeconds;
        var time = sec / limitCycle;

        // 根据流量大小做限制
        var key = $"flow:{ip}:{time}";
        var size = _cacheProvider.Cache.Increment(key, entry.Size);
        if (size <= entry.Size)
            _cacheProvider.Cache.SetExpire(key, TimeSpan.FromSeconds(limitCycle));

        DefaultSpan.Current?.AppendTag($"ValidLimit: time={TimeSpan.FromSeconds(time * limitCycle)} size={size}");

        // 避免第一个文件都无法下载
        if (size - entry.Size > maxSize)
        {
            _tracer?.NewError("access:FlowLimit", new { entry.Path, entry.Size, TotalSize = size });

            return false;
        }

        return true;
    }
}
