using System.IO;
using System.Security.Cryptography;
using EasyWeb.Data;
using NewLife;
using NewLife.Caching;
using NewLife.Http;
using NewLife.Log;

namespace EasyWeb.Services;

public class EntryService
{
    private readonly ICacheProvider _cacheProvider;

    public EntryService(ICacheProvider cacheProvider) => _cacheProvider = cacheProvider;

    /// <summary>获取默认存储</summary>
    /// <returns></returns>
    public FileStorage GetDefaultStorage()
    {
        return _cacheProvider.Cache.GetOrAdd($"DefaultStorage",
            k => FileStorage.FindAllWithCache().FirstOrDefault(e => e.Enable), 60);
    }

    public IList<FileEntry> GetEntries(Int32 storageId, Int32 parentId)
    {
        if (storageId == 0) storageId = GetDefaultStorage()?.Id ?? 0;

        return _cacheProvider.Cache.GetOrAdd($"Entries:{storageId}:{parentId}",
            k => FileEntry.Search(storageId, parentId, true), 60);
    }

    public FileEntry GetEntry(Int32 storageId, String path)
    {
        if (path.IsNullOrEmpty()) return null;

        if (storageId == 0) storageId = GetDefaultStorage()?.Id ?? 0;

        return _cacheProvider.Cache.GetOrAdd($"Entry:{storageId}:{path}",
            k => FileEntry.FindByStorageIdAndPath(storageId, path), 60);
    }

    /// <summary>获取文件，增加访问量。可能指向真实文件</summary>
    /// <param name="storageId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public FileEntry RetrieveFile(Int32 storageId, String path)
    {
        var entry = GetEntry(storageId, path);
        if (entry == null || !entry.Enable) return null;

        // 增加浏览数
        entry.Times++;
        entry.LastDownload = DateTime.Now;
        entry.SaveAsync(15_000);

        // 可能是链接文件，使用目标文件
        if (!entry.LinkTarget.IsNullOrEmpty())
        {
            entry = GetLink(entry);
            if (entry == null || !entry.Enable) return null;

            entry.Times++;
            entry.LastDownload = DateTime.Now;
            entry.SaveAsync(15_000);
        }

        return entry;
    }

    public Boolean CheckHash(FileEntry entry, FileInfo fi)
    {
        // 校验哈希信息
        if (!entry.Hash.IsNullOrEmpty())
        {
            XTrace.WriteLine("校验文件哈希：{0} {1}", fi.FullName, entry.Hash);

            using var fs = fi.OpenRead();
            var hash = SHA512.Create().ComputeHash(fs).ToHex().ToLower();
            fs.TryDispose();

            if (hash != entry.Hash)
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

        XTrace.WriteLine("文件不存在，准备下载 {0} => {1}", url, path);

        var client = new HttpClient();
        await client.DownloadFileAsync(url, path);

        if (!System.IO.File.Exists(path)) return false;

        var fi = path.AsFile();
        XTrace.WriteLine("下载完成，大小：{0}", fi.Length.ToGMK());

        // 校验哈希信息
        if (!CheckHash(entry, fi))
        {
            fi.Delete();
            return false;
        }

        entry.Size = fi.Length;
        entry.LastWrite = DateTime.Now;
        entry.LastAccess = DateTime.Now;
        entry.Update();

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

        // 带有*的模糊匹配，先截断路径为目录和文件，*只能在文件名中
        var p = link.LastIndexOf('/');
        if (p < 0)
            throw new ArgumentOutOfRangeException(nameof(entry.LinkTarget), "链接目标不合法 " + entry.LinkTarget);

        var dir = link[..p];
        var file = link[(p + 1)..];
        if (dir.Contains('*'))
            throw new ArgumentOutOfRangeException(nameof(entry.LinkTarget), "链接目标不合法 " + entry.LinkTarget);

        // 查找目录
        var parent = GetEntry(entry.StorageId, dir);
        if (parent == null) return null;

        var childs = GetEntries(entry.StorageId, parent.Id);
        childs = childs.Where(e => file.IsMatch(e.Name)).ToList();
        if (childs.Count == 0) return null;

        // 返回最新的文件
        return childs.OrderByDescending(e => e.LastWrite).FirstOrDefault();
    }
}
