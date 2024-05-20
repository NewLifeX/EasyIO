using System.Reflection.Metadata;
using EasyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube;
using NewLife.Web;
using XCode;

namespace NewLife.EasyWeb.Controllers;

/// <summary>文件控制器</summary>
public class HomeController : ControllerBaseX
{
    private readonly EntryService _entryService;

    public HomeController(EntryService entryService)
    {
        _entryService = entryService;

        PageSetting.EnableNavbar = false;
    }

    /// <summary>主页面</summary>
    /// <returns></returns>
    public ActionResult Index()
    {
        var page = new Pager(WebHelper.Params);
        var parentId = page["parentId"].ToInt();

        //var list = FileStorage.FindAllWithCache().FirstOrDefault(e => e.Enable);
        //var entris = FileEntry.FindAllByStorageIdAndParentId(list.Id, 0).Where(e => e.Enable).ToList();

        var entris = _entryService.GetFiles(0, parentId);

        // 目录优先，然后按照名称排序
        entris = entris.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name).ToList();

        //PageSetting.EnableNavbar = false;
        ViewBag.Title = "文件";

        return View(entris);
    }

    /// <summary>下载文件</summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public ActionResult DownloadFile(String file)
    {
        var entry = _entryService.GetFile(0, file);
        if (entry == null) return NotFound();

        // 增加浏览数
        entry.Times++;
        entry.LastDownload = DateTime.Now;
        entry.SaveAsync(15_000);

        return View("Info", entry);
    }
}