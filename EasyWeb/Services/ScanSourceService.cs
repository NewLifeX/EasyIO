
using EasyWeb.Data;
using NewLife;
using NewLife.Threading;

namespace EasyWeb.Services;

public class ScanSourceService : IHostedService
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
        var list = FileSource.FindAll();
        foreach (var item in list)
        {
            if (!item.Enable) continue;

            var period = item.Period;
            if (period <= 0) period = 86400;

            if (item.LastScan.AddSeconds(period) < DateTime.Now)
            {
                Process(item);

                item.LastScan = DateTime.Now;
                item.Save();
            }
        }
    }

    void Process(FileSource entity)
    {
    }
}
