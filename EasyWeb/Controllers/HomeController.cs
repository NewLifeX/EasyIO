using EasyWeb.Data;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube;

namespace NewLife.EasyWeb.Controllers;

/// <summary>文件控制器</summary>
public class HomeController : ControllerBaseX
{
    /// <summary>主页面</summary>
    /// <returns></returns>
    public ActionResult Index()
    {
        var list = FileStorage.FindAllWithCache().FirstOrDefault(e => e.Enable);
        var entris = FileEntry.FindAllByStorageIdAndParentId(list.Id, 0).Where(e => e.Enable).ToList();

        // 目录优先，然后按照名称排序
        entris = entris.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name).ToList();

        PageSetting.EnableNavbar = false;
        ViewBag.Title = "文件";

        return View(entris);
    }
}