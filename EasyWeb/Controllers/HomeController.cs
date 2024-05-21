using System.IO;
using System.Security.Cryptography;
using System.Web;
using EasyWeb.Data;
using EasyWeb.Models;
using EasyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife.Caching;
using NewLife.Cube;
using NewLife.Http;
using NewLife.Log;

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

        var pid = 0;
        var parent = _entryService.GetFile(0, pathInfo);
        if (parent != null && parent.Enable) pid = parent.Id;

        var entris = _entryService.GetFiles(0, pid);

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
    public async Task<ActionResult> DownloadFile(String pathInfo)
    {
        if (pathInfo.IsNullOrEmpty()) return NotFound();

        pathInfo = HttpUtility.UrlDecode(pathInfo);

        var entry = _entryService.GetFile(0, pathInfo);
        if (entry == null || !entry.Enable) return NotFound();

        // 增加浏览数
        entry.Times++;
        entry.LastDownload = DateTime.Now;
        entry.SaveAsync(15_000);

        // 组装本地路径
        var path = entry.FullName;
        if (entry.Storage != null) path = entry.Storage.HomeDirectory.CombinePath(entry.Path);

        path = path.GetFullPath();

        var last = entry.LastWrite;
        if (last.Year < 2000) last = entry.UpdateTime;

        var fi = path.AsFile();
        if (fi.Exists && _cacheProvider.InnerCache.Add($"hash:{entry.Id}", entry.Path, 600))
        {
            // 校验哈希信息
            if (!CheckHash(entry, fi))
            {
                fi.Delete();
            }
        }

        // 如果文件不存在，则临时下载，或者返回404
        if (!System.IO.File.Exists(path))
        {
            var url = entry.RawUrl;
            if (url.IsNullOrEmpty()) return NotFound();

            XTrace.WriteLine("文件不存在，准备下载 {0} => {1}", url, path);

            var client = new HttpClient();
            await client.DownloadFileAsync(url, path);

            if (!System.IO.File.Exists(path)) return NotFound();

            fi.Refresh();
            XTrace.WriteLine("下载完成，大小：{0}", fi.Length.ToGMK());

            // 校验哈希信息
            if (!CheckHash(entry, fi))
            {
                fi.Delete();
                return NotFound();
            }

            entry.Size = fi.Length;
            entry.LastWrite = DateTime.Now;
            entry.LastAccess = DateTime.Now;
            entry.Update();
        }

        return PhysicalFile(path, "application/octet-stream", HttpUtility.HtmlEncode(entry.Name), last, null, true);
    }

    Boolean CheckHash(FileEntry entry, FileInfo fi)
    {
        // 校验哈希信息
        if (!entry.Hash.IsNullOrEmpty())
        {
            XTrace.WriteLine("校验文件哈希：{0} {1}", fi.FullName, entry.Hash);

            using var fs = fi.OpenRead();
            var hash = SHA512.Create().ComputeHash(fs).ToHex().ToLower();
            fs.TryDispose();

            if (hash != entry.Hash)
            {
                XTrace.WriteLine("文件哈希不一致 {0} {1}", hash, entry.Hash);
                return false;
            }
        }

        return true;
    }
}