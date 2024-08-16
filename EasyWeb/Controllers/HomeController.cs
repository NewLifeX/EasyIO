using System.Net;
using System.Web;
using EasyWeb.Models;
using EasyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife.Caching;
using NewLife.Cube;
using NewLife.Log;
using NewLife.Web;

namespace NewLife.EasyWeb.Controllers;

/// <summary>文件控制器</summary>
public class HomeController : ControllerBaseX
{
    private readonly EntryService _entryService;
    private readonly ICacheProvider _cacheProvider;
    private readonly ITracer _tracer;

    public HomeController(EntryService entryService, ICacheProvider cacheProvider, ITracer tracer)
    {
        _entryService = entryService;
        _cacheProvider = cacheProvider;
        _tracer = tracer;
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

        var p = new Pager(WebHelper.Params);
        entris = (p["sort"] + "") switch
        {
            "time" => entris.OrderByDescending(e => e.IsDirectory)
                               .ThenByDescending(e => e.LastWrite)
                               .ThenBy(e => e.Name).ToList(),
            "size" => entris.OrderByDescending(e => e.IsDirectory)
                               .ThenByDescending(e => e.Size)
                               .ThenBy(e => e.Name).ToList(),
            "name" => entris.OrderByDescending(e => e.IsDirectory)
                               .ThenBy(e => e.Name).ToList(),
            _ => entris.OrderByDescending(e => e.IsDirectory)
                               .ThenByDescending(e => !e.Tag.IsNullOrEmpty())
                               .ThenBy(e => e.Tag + "")
                               .ThenByDescending(e => e.Version)
                               //.ThenByDescending(e => e.LastWrite)
                               .ThenBy(e => e.Name).ToList(),
        };

        // 目录优先，然后按照名称排序
        //entris = entris.OrderByDescending(e => e.IsDirectory).ThenBy(e => e.Name).ToList();
        //entris = entris.OrderByDescending(e => e.IsDirectory).ThenByDescending(e => e.LastWrite).ThenBy(e => e.Name).ToList();
        //entris = entris.OrderByDescending(e => e.IsDirectory)
        //    .ThenByDescending(e => !e.Tag.IsNullOrEmpty())
        //    .ThenBy(e => e.Tag + "")
        //    .ThenByDescending(e => e.Version)
        //    .ThenByDescending(e => e.LastWrite)
        //    .ThenBy(e => e.Name).ToList();

        var cdnIp = Request.Headers["Ali-Cdn-Real-Ip"] + "";
        var isCdn = !cdnIp.IsNullOrEmpty();
        var isHttps = Request.IsHttps;

        var model = new DirectoryModel();

        var ps = new List<FileModel>();
        var pt = parent;
        while (pt != null)
        {
            ps.Add(_entryService.BuildModel(pt, !isCdn, isHttps));
            pt = pt.Parent;
        }
        ps.Add(new FileModel { Path = "" });
        model.Parents = ps;

        model.Entries = entris.Select(e => _entryService.BuildModel(e, !isCdn, isHttps)).ToList();

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
        if (pathInfo.IsNullOrEmpty()) return NotFound("路径为空");

        pathInfo = HttpUtility.UrlDecode(pathInfo);

        var (entry, link) = _entryService.RetrieveFile(0, pathInfo);
        if (entry == null || !entry.Enable) return NotFound("未找到文件清单");

        using var span = _tracer?.NewSpan(nameof(DownloadFile), new { entry.Name, entry.Path, entry.FullName });

        // 链接跳转到目标
        if (link != null && entry.LinkRedirect)
            return Redirect(link.Path.EnsureStart("/"));

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
                _tracer?.NewError("DeleteFile-HashError", $"{fe.Path} {fi.FullName} {fe.Hash}");
                //fi.Delete();

                _cacheProvider.InnerCache.Remove($"hash:{fe.Id}");
            }
        }

        // 直接跳转到原始地址
        var mode = entry?.RedirectMode;
        if (mode == null || mode == RedirectModes.None) mode = entry.Parent?.RedirectMode;
        if (mode == null || mode == RedirectModes.None) mode = entry.Storage?.RedirectMode;
        if (mode == RedirectModes.Smart)
        {
            // 智能跳转。验证通过的不需要跳转
            var auth_key = Request.Query["auth_key"] + "";
            if (auth_key.IsNullOrEmpty()) mode = RedirectModes.Redirect;
        }

        var url = entry.RawUrl;
        if (!url.IsNullOrEmpty() && mode != null && mode == RedirectModes.Redirect)
        {
            // 是否需要修改协议
            if (entry.Source != null && !entry.Source.Protocol.IsNullOrEmpty())
            {
                var p = url.IndexOf("://");
                if (p > 0)
                {
                    url = entry.Source.Protocol + url[p..];
                }
            }

            return Redirect(url);
        }

        // 如果文件不存在，则临时下载，或者返回404
        if (!System.IO.File.Exists(path))
        {
            try
            {
                if (!await _entryService.DownloadAsync(fe, path))
                    return NotFound($"下载失败!id={fe.Id}/{fe.Name}");
            }
            catch (Exception ex)
            {
                DefaultSpan.Current?.SetError(ex, fe.Path);

                return NotFound($"下载异常!id={fe.Id}/{fe.Name}");
            }
        }

        // 根据流量大小做限制
        if (!_entryService.ValidLimit(entry, UserHost, 600, 100 * 1024 * 1024))
        {
            return Problem("流量超限", null, (Int32?)HttpStatusCode.TooManyRequests, "Title", "Type");
        }

        // 文件下载使用原始访问的名字和时间
        var last = entry.LastWrite;
        if (last.Year < 2000) last = entry.UpdateTime;

        return PhysicalFile(path, "application/octet-stream", HttpUtility.HtmlEncode(entry.Name), last, null, true);
    }
}