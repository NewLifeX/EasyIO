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
app.MapControllerRoute(name: "default", pattern: "{controller=Index}/{action=Index}/{id?}");

// 注册退出事件
if (app is IHost host)
    NewLife.Model.Host.RegisterExit(() => host.StopAsync().Wait());

app.Run();