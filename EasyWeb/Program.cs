using EasyWeb;
using EasyWeb.Services;
using NewLife.Cube;
using NewLife.EasyWeb;
using NewLife.Log;

XTrace.UseConsole();

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var star = services.AddStardust(null);

var set = EasyFileSetting.Current;
services.AddSingleton(set);

services.AddSingleton<EntryService>();
services.AddSingleton<ScanStorageService>();
services.AddSingleton<ScanSourceService>();
services.AddSingleton<DirUrlConstraint>();
services.AddSingleton<FileUrlConstraint>();

services.AddHostedService<ScanStorageService>();
services.AddHostedService<ScanSourceService>();

services.AddControllersWithViews();
services.AddEasyIO();
services.AddCube();

var app = builder.Build();
app.UseStaticFiles();
//app.UseStardust();
app.UseCube(app.Environment);
app.UseEasyIO(true);
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "ShowDir",
    pattern: "{*pathInfo}",
    defaults: new { controller = "Home", action = "ShowDir" },
    constraints: new { file = app.Services.GetRequiredService<DirUrlConstraint>() }
);

app.MapControllerRoute(
    name: "Download",
    pattern: "{*pathInfo}",
    defaults: new { controller = "Home", action = "DownloadFile" },
    constraints: new { file = app.Services.GetRequiredService<FileUrlConstraint>() }
);

// 注册退出事件
if (app is IHost host)
    NewLife.Model.Host.RegisterExit(() => host.StopAsync().Wait());

app.Run();