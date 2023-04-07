using NewLife.EasyWeb;
using NewLife.Log;

XTrace.UseConsole();

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var star = services.AddStardust(null);

services.AddControllersWithViews();
services.AddEasyIO();

var app = builder.Build();
app.UseStaticFiles();
app.UseStardust();
app.UseEasyIO(true);
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Index}/{action=Index}/{id?}");

app.Run();