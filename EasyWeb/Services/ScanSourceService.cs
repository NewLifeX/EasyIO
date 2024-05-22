
using System.Security.Cryptography;
using EasyWeb.Data;
using EasyWeb.Models;
using NewLife;
using NewLife.Log;
using NewLife.Serialization;
using NewLife.Threading;
using XCode;

namespace EasyWeb.Services;

public class ScanSourceService : IHostedService
{
    private TimerX _timer;
    private readonly EntryService _entryService;
    private readonly ITracer _tracer;

    public ScanSourceService(EntryService entryService, ITracer tracer)
    {
        _entryService = entryService;
        _tracer = tracer;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _timer = new TimerX(DoScan, null, 3_000, 60_000);

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _timer.TryDispose();

        return Task.CompletedTask;
    }

    void DoScan(Object state)
    {
        using var span = _tracer?.NewSpan("ScanSource");

        var list = FileSource.FindAll();
        foreach (var item in list)
        {
            if (!item.Enable) continue;

            var period = item.Period;
            if (period <= 0) continue;

            if (item.LastScan.AddSeconds(period) < DateTime.Now)
            {
                if (item.Kind.EqualIgnoreCase("dotNet"))
                    ScanDotNet(item).Wait();

                item.LastScan = DateTime.Now;

                item.Save();
            }
        }

        // 创建链接文件
        if (list.Count > 0)
            CreateLinks(list[0].StorageId);
    }

    public async Task ScanDotNet(FileSource source)
    {
        if (source.Url.IsNullOrEmpty()) return;

        using var span = _tracer?.NewSpan(nameof(ScanDotNet));

        XTrace.WriteLine("扫描：{0}", source.Name);

        // 查找dotNet根目录，不存在则创建
        var sid = source.StorageId;
        var root = FileEntry.FindAllByStorageIdAndPath(sid, "dotNet").FirstOrDefault();
        if (root == null)
        {
            root = new FileEntry
            {
                SourceId = source.Id,
                StorageId = sid,
                Name = "dotNet",
                Path = "dotNet",
                Enable = true,
                IsDirectory = true,
            };
            root.Insert();
        }

        try
        {
            var http = new HttpClient();
            var rs = await http.GetStringAsync(source.Url);
            if (rs.IsNullOrEmpty()) return;

            var js = JsonParser.Decode(rs);
            if (js == null || js.Count == 0) return;

            var releases = js["releases"] as IList<Object>;
            if (releases == null || releases.Count == 0) return;

            var childs = FileEntry.FindAllByParentId(root.Id);
            foreach (var item in releases)
            {
                if (item is IDictionary<String, Object> rel)
                {
                    var ver = rel["release-version"] + "";
                    if (ver.IsNullOrEmpty()) continue;

                    // 版本目录。每个版本一个目录
                    var fe = childs.FirstOrDefault(e => e.Name == ver);

                    // 黑白名单过滤
                    if (!source.IsMatch(ver))
                    {
                        if (fe != null)
                        {
                            fe.Enable = false;
                            fe.Update();
                        }

                        continue;
                    }

                    using var span2 = _tracer?.NewSpan("ScanDotNet-Release", ver);
                    var url = rel["release-notes"] + "";
                    try
                    {
                        fe ??= new FileEntry { Name = ver };
                        fe.SourceId = root.Id;
                        fe.StorageId = root.StorageId;
                        fe.ParentId = root.Id;
                        fe.Path = $"{root.Path}/{ver}";
                        fe.IsDirectory = true;
                        fe.Enable = true;
                        fe.RawUrl = url;
                        fe.LastWrite = rel["release-date"].ToDateTime();

                        _entryService.FixVersionAndTag(fe);

                        fe.Save();

                        Process(source, fe, rel["runtime"]);
                        Process(source, fe, rel["sdk"]);
                        Process(source, fe, rel["aspnetcore-runtime"]);
                        Process(source, fe, rel["windowsdesktop"]);
                    }
                    catch (Exception ex)
                    {
                        span2?.SetError(ex, null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }

    void Process(FileSource source, FileEntry parent, Object release)
    {
        if (release == null) return;

        var rver = JsonHelper.Convert<ReleaseVersion>(release);
        if (rver == null || rver.Version.IsNullOrEmpty()) return;
        if (rver.Files == null || rver.Files.Count == 0) return;

        XTrace.WriteLine("分析：{0}", rver.Version);

        var list = new List<FileEntry>();
        var childs = FileEntry.FindAllByParentId(parent.Id);
        foreach (var item in rver.Files)
        {
            var name = Path.GetFileName(item.Url);
            if (name.IsNullOrEmpty()) continue;

            var fe = childs.FirstOrDefault(e => e.Name.EqualIgnoreCase(name));

            // 黑白名单
            if (!source.IsMatch(name))
            {
                if (fe != null)
                {
                    fe.Enable = false;
                    fe.Update();
                }

                continue;
            }

            if (fe != null)
                childs.Remove(fe);
            else
                fe = new FileEntry { Name = name };

            fe.SourceId = parent.Id;
            fe.StorageId = parent.StorageId;
            fe.ParentId = parent.Id;

            fe.Name = name;
            fe.Path = $"{parent.Path}/{name}";
            fe.RawUrl = item.Url;
            fe.Hash = item.Hash;
            fe.Enable = true;
            fe.Remark = item.Rid;

            // 解析版本号
            _entryService.FixVersionAndTag(fe);

            if (fe.LastWrite.Year < 2000) fe.LastWrite = parent.LastWrite;

            if ((fe as IEntity).HasDirty)
                fe.LastScan = DateTime.Now;

            //fe.Save();
            list.Add(fe);
        }
        list.Save();

        // childs中有而root中没有的，需要标记禁用，长时间禁用的，需要标记已删除
        foreach (var fe in childs)
        {
            if (fe.LastScan.AddDays(7) < DateTime.Now)
            {
                fe.Enable = false;
                fe.Save();
            }
            else if (fe.UpdateTime.AddDays(30) < DateTime.Now)
            {
                //todo 暂时不要删除
                //fe.Delete();
            }
        }

        //todo 增加一个定时任务，定时检查文件是否过期，如果已经过期（7天），则删除文件，更近一步是禁用（180天），紧接着删除（360天）
    }

    public void CreateLinks(Int32 storageId)
    {
        var root = FileEntry.FindAllByStorageIdAndPath(storageId, "dotNet").FirstOrDefault();
        if (root == null) return;

        var childs = FileEntry.FindAllByParentId(root.Id);

        var kinds = new[] { "aspnetcore-runtime", "dotnet-runtime", "windowsdesktop-runtime", "dotnet-hosting" };
        var rids = new[] {
            "win-x86", "win-x64", "win-arm64", "win",
            "linux-x64", "linux-arm", "linux-arm64",
            "linux-musl-x64", "linux-musl-arm", "linux-musl-arm64",
            "osx-x64", "osx-arm64",
        };

        var last = childs.OrderByDescending(e => e.Version).FirstOrDefault();
        var childs2 = FileEntry.FindAllByParentId(last?.Id ?? -1);
        var tags = childs2.Where(e => !e.Tag.IsNullOrEmpty()).Select(e => e.Tag).Distinct().ToList();

        // 遍历所有组合
        foreach (var k in kinds)
        {
            foreach (var r in rids)
            {
                var tag = $"{k}-{r}";
                if (tags.Contains(tag))
                {
                    var ext = "";
                    if (r.StartsWith("win"))
                        ext = ".exe";
                    else if (r.StartsWith("linux-"))
                        ext = ".tar.gz";
                    else if (r.StartsWith("osx-"))
                        ext = ".pkg";

                    var name = tag + ext;

                    var fe = childs.FirstOrDefault(e => e.Name.EqualIgnoreCase(name));
                    fe ??= new FileEntry { Name = name };

                    fe.StorageId = root.StorageId;
                    fe.ParentId = root.Id;
                    fe.Name = name;
                    fe.Tag = tag;
                    fe.Path = $"{root.Path}/{name}";
                    fe.Enable = true;
                    fe.IsDirectory = false;

                    fe.LinkTarget = $"{root.Name}/!*-*/{k}-*-{r}{ext}";
                    fe.LinkRedirect = true;

                    fe.Save();
                }
            }
        }
    }
}
