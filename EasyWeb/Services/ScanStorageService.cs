
using EasyWeb.Data;
using NewLife;
using NewLife.Threading;

namespace EasyWeb.Services;

public class ScanStorageService : IHostedService
{
    private TimerX _timer;
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
        var list = FileStorage.FindAll();
        foreach (var item in list)
        {
            if (!item.Enable) continue;

            var period = item.Period;
            if (period <= 0) period = 86400;

            if (item.LastScan.AddSeconds(period) < DateTime.Now)
            {
                Process(item, null, null);

                item.LastScan = DateTime.Now;
                item.Save();
            }
        }
    }

    void Process(FileStorage storage, DirectoryInfo root, FileEntry parent)
    {
        var pid = parent?.Id ?? 0;
        if (pid == 0)
        {
            // 根目录是否存在
            root = storage.HomeDirectory.AsDirectory();
        }
        if (!root.Exists) return;

        var pattern = storage.Pattern;
        if (pattern.IsNullOrEmpty()) pattern = "*";

        var childs = FileEntry.FindAllByStorageIdAndParentId(storage.Id, pid);

        foreach (var fi in root.GetFiles(storage.Pattern))
        {
            var fe = childs.FirstOrDefault(e => e.Name == fi.Name);
            fe ??= new FileEntry { Name = fi.Name };

            fe.StorageId = storage.Id;
            fe.ParentId = pid;
            fe.Enable = true;
            fe.FullName = fi.FullName;
            fe.Size = fi.Length;
            fe.Hash = fi.MD5().ToHex();
            fe.LastWrite = fi.LastWriteTime;
            fe.LastAccess = fi.LastAccessTime;
            fe.LastScan = DateTime.Now;

            fe.Save();
        }

        foreach (var di in root.GetDirectories(storage.Pattern))
        {
            var fe = childs.FirstOrDefault(e => e.Name == di.Name);
            fe ??= new FileEntry { Name = di.Name };

            fe.StorageId = storage.Id;
            fe.ParentId = pid;
            fe.Enable = true;
            fe.IsDirectory = true;
            fe.FullName = di.FullName;
            fe.LastScan = DateTime.Now;

            fe.Save();

            Process(storage, di, fe);
        }

        //todo childs中有而root中没有的，需要标记禁用，长时间禁用的，需要标记已删除
    }
}
