
using EasyWeb.Data;
using NewLife;
using NewLife.Log;
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
            if (period <= 0) continue;

            if (item.LastScan.AddSeconds(period) < DateTime.Now)
            {
                Process(item, null, null);

                item.LastScan = DateTime.Now;
                item.Save();
            }
        }
    }

    void Process(FileStorage storage, DirectoryInfo parentDir, FileEntry parent)
    {
        var root = storage.HomeDirectory.AsDirectory();

        var pid = parent?.Id ?? 0;
        if (pid == 0)
        {
            // 根目录是否存在
            parentDir = root;
        }
        if (!parentDir.Exists) return;

        var pattern = storage.Pattern;
        if (pattern.IsNullOrEmpty()) pattern = "*";

        XTrace.WriteLine("扫描目录：{0}", parentDir.FullName);

        var childs = FileEntry.FindAllByStorageIdAndParentId(storage.Id, pid);

        var totalSize = 0L;
        var rootPath = root.FullName.EnsureEnd(Runtime.Windows ? "\\" : "/");
        foreach (var fi in parentDir.GetFiles(pattern))
        {
            var fe = childs.FirstOrDefault(e => e.Name == fi.Name);
            if (fe == null)
                fe = new FileEntry { Name = fi.Name };
            else
                childs.Remove(fe);

            fe.StorageId = storage.Id;
            fe.ParentId = pid;
            fe.Enable = true;
            fe.FullName = fi.FullName.TrimStart(rootPath);
            fe.Size = fi.Length;
            fe.Hash = fi.MD5().ToHex();
            fe.LastWrite = fi.LastWriteTime;
            fe.LastAccess = fi.LastAccessTime;
            fe.LastScan = DateTime.Now;

            fe.Save();

            totalSize += fe.Size;
        }

        foreach (var di in parentDir.GetDirectories(pattern))
        {
            var fe = childs.FirstOrDefault(e => e.Name == di.Name);
            if (fe == null)
                fe = new FileEntry { Name = di.Name };
            else
                childs.Remove(fe);

            fe.StorageId = storage.Id;
            fe.ParentId = pid;
            fe.Enable = true;
            fe.IsDirectory = true;
            fe.FullName = di.FullName.TrimStart(rootPath);
            fe.LastScan = DateTime.Now;

            fe.Save();

            Process(storage, di, fe);

            totalSize += fe.Size;
        }

        if (parent != null)
        {
            parent.Size = totalSize;
            parent.Update();
        }
        else
        {
            storage.Size = totalSize;
            storage.Update();
        }

        // childs中有而root中没有的，需要标记禁用，长时间禁用的，需要标记已删除
        foreach (var fe in childs)
        {
            if (fe.Enable)
            {
                fe.Enable = false;
                fe.Save();
            }
            else if (fe.UpdateTime.AddDays(30) < DateTime.Now)
            {
                fe.Delete();
            }
        }
    }
}
