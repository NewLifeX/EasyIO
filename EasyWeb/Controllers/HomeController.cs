using System.Web;
using EasyWeb.Data;
using EasyWeb.Models;
using EasyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife.Cube;
using NewLife.Web;

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
        return ShowDir(null);
    }

    /// <summary>显示目录</summary>
    /// <param name="pathInfo"></param>
    /// <returns></returns>
    public ActionResult ShowDir(String pathInfo)
    {
        if (!pathInfo.IsNullOrEmpty())
            pathInfo = HttpUtility.UrlDecode(pathInfo);

        var parent = _entryService.GetFile(0, pathInfo);
        var entris = _entryService.GetFiles(0, parent?.Id ?? 0);

        // 目录优先，然后按照名称排序
        entris = entris.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name).ToList();

        var model = new DirectoryModel
        {
            Parent = parent,
            Entries = entris,
        };

        if (parent != null)
            ViewBag.Title = parent.Path;
        else
            ViewBag.Title = "Home";

        return View("Index", model);
    }

    /// <summary>下载文件</summary>
    /// <param name="pathInfo"></param>
    /// <returns></returns>
    public ActionResult DownloadFile(String pathInfo)
    {
        if (pathInfo.IsNullOrEmpty()) return NotFound();

        pathInfo = HttpUtility.UrlDecode(pathInfo);

        var entry = _entryService.GetFile(0, pathInfo);
        if (entry == null) return NotFound();

        // 增加浏览数
        entry.Times++;
        entry.LastDownload = DateTime.Now;
        entry.SaveAsync(15_000);

        // 组装本地路径
        var path = entry.FullName;
        if (entry.Storage != null) path = entry.Storage.HomeDirectory.CombinePath(entry.Path);

        path = path.GetFullPath();

        return PhysicalFile(path, "application/octet-stream", HttpUtility.HtmlEncode(entry.Name), entry.LastWrite, null, true);
    }
}