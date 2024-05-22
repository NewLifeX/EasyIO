using System.Web;
using EasyWeb.Models;
using EasyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife.Caching;
using NewLife.Cube;

namespace NewLife.EasyWeb.Controllers;

/// <summary>文件控制器</summary>
public class HomeController : ControllerBaseX
{
    private readonly EntryService _entryService;
    private readonly ICacheProvider _cacheProvider;

    public HomeController(EntryService entryService, ICacheProvider cacheProvider)
    {
        _entryService = entryService;
        _cacheProvider = cacheProvider;
        PageSetting.EnableNavbar = false;
    }

    /// <summary>主页面</summary>
    /// <returns></returns>
    public ActionResult Index() => ShowDir(null);

    /// <summary>显示目录</summary>
    /// <param name="pathInfo"></param>
    /// <returns></returns>
    public ActionResult ShowDir(String pathInfo)
    {
        if (!pathInfo.IsNullOrEmpty())
            pathInfo = HttpUtility.UrlDecode(pathInfo);

        var pid = 0;
        var parent = _entryService.GetEntry(0, pathInfo);
        if (parent != null && parent.Enable) pid = parent.Id;

        var entris = _entryService.GetEntries(0, pid);

        // 目录优先，然后按照名称排序
        entris = entris.OrderByDescending(e => e.IsDirectory).ThenByDescending(e => e.Name).ToList();

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
    public async Task<ActionResult> DownloadFile(String pathInfo)
    {
        if (pathInfo.IsNullOrEmpty()) return NotFound();

        pathInfo = HttpUtility.UrlDecode(pathInfo);

        var (entry, link) = _entryService.RetrieveFile(0, pathInfo);
        if (entry == null || !entry.Enable) return NotFound();

        var fe = link ?? entry;

        // 组装本地路径
        var path = fe.FullName;
        if (fe.Storage != null) path = fe.Storage.HomeDirectory.CombinePath(fe.Path);

        path = path.GetFullPath();

        var fi = path.AsFile();
        if (fi.Exists && _cacheProvider.InnerCache.Add($"hash:{fe.Id}", fe.Path, 600))
        {
            // 校验哈希信息
            if (!_entryService.CheckHash(fe, fi))
            {
                fi.Delete();

                _cacheProvider.InnerCache.Remove($"hash:{fe.Id}");
            }
        }

        // 如果文件不存在，则临时下载，或者返回404
        if (!System.IO.File.Exists(path))
        {
            if (!await _entryService.DownloadAsync(fe, path))
                return NotFound();
        }

        // 文件下载使用原始访问的名字和时间
        var last = entry.LastWrite;
        if (last.Year < 2000) last = entry.UpdateTime;

        return PhysicalFile(path, "application/octet-stream", HttpUtility.HtmlEncode(entry.Name), last, null, true);
    }
}