
using EasyWeb.Data;
using NewLife;
using NewLife.Log;
using NewLife.Remoting.Models;
using NewLife.Threading;
using XCode;

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
                Scan(item, null, null, 1);

                item.LastScan = DateTime.Now;
                item.Save();
            }
        }
    }

    /// <summary>扫描指定目录</summary>
    /// <param name="storage">存储区</param>
    /// <param name="target">目标目录</param>
    /// <param name="parent">父级清单</param>
    /// <param name="level">当前层级</param>
    public void Scan(FileStorage storage, DirectoryInfo target, FileEntry parent, Int32 level)
    {
        if (storage.Level > 0 && level > storage.Level) return;

        var root = storage.HomeDirectory.AsDirectory();

        var pid = parent?.Id ?? 0;
        if (pid == 0)
        {
            // 根目录是否存在
            target = root;
        }
        if (!target.Exists) return;

        // 设置状态为处理中
        if (parent == null)
        {
            storage.Status = CommandStatus.处理中;
            storage.Update();
        }

        var pattern = storage.Pattern;
        if (pattern.IsNullOrEmpty()) pattern = "*";

        XTrace.WriteLine("扫描目录：{0}", target.FullName);

        var childs = FileEntry.Search(storage.Id, pid, null);

        var maxLength = 250;
        var totalSize = 0L;
        var rootPath = root.FullName.EnsureEnd(Runtime.Windows ? "\\" : "/");

        // 扫描文件
        foreach (var fi in target.GetFiles(pattern))
        {
            // 跳过太长的文件
            if (fi.Name.Length > maxLength) continue;

            try
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
                fe.Path = parent != null ? $"{parent.Path}/{fe.Name}" : fe.Name;
                fe.Size = fi.Length;
                fe.LastWrite = fi.LastWriteTime;
                fe.LastAccess = fi.LastAccessTime;
                try
                {
                    fe.Hash = fi.MD5().ToHex();
                }
                catch { }

                if (fe.FullName.Length > maxLength || fe.Path.Length > maxLength) continue;

                if ((fe as IEntity).HasDirty || fe.LastScan.Date != DateTime.Today)
                {
                    fe.LastScan = DateTime.Now;
                    fe.Save();
                }

                // 更新目录的最后修改时间，层层叠加，让上级目录知道内部有文件被修改
                if (parent != null && parent.LastWrite < fe.LastWrite) parent.LastWrite = fe.LastWrite;

                totalSize += fe.Size;
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }
        }

        if (storage.Level <= 0 || level < storage.Level)
        {
            // 扫描目录
            foreach (var di in target.GetDirectories(pattern))
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
                fe.Path = parent != null ? $"{parent.Path}/{fe.Name}" : fe.Name;

                if (fe.FullName.Length > maxLength || fe.Path.Length > maxLength) continue;

                if (fe.Id == 0) fe.Insert();

                Scan(storage, di, fe, level + 1);

                if ((fe as IEntity).HasDirty || fe.LastScan.Date != DateTime.Today)
                {
                    fe.LastScan = DateTime.Now;
                    fe.Save();
                }

                // 更新目录的最后修改时间，层层叠加，让上级目录知道内部有文件被修改
                if (parent != null && parent.LastWrite < fe.LastWrite) parent.LastWrite = fe.LastWrite;

                totalSize += fe.Size;
            }
        }

        if (parent != null)
        {
            parent.Size = totalSize;
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

        // 设置状态为已完成
        if (parent == null)
        {
            storage.Size = totalSize;
            storage.Status = CommandStatus.已完成;
            storage.Update();

            XTrace.WriteLine("扫描完成！{0}", target.FullName);
        }
    }
}
