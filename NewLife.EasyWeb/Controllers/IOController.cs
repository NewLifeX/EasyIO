using System.IO;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.EasyIO.Options;
using NewLife.Log;

namespace NewLife.EasyWeb.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class IOController : ApiControllerBase
{
    private readonly FileStorageOptions _storageOptions;
    private readonly ITracer _tracer;

    public IOController(FileStorageOptions storageOptions, ITracer tracer)
    {
        _storageOptions = storageOptions;
        _tracer = tracer;
    }

    private String GetFile(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var set = _storageOptions;
        if (set.Path.IsNullOrEmpty()) throw new Exception("未配置存储信息");

        var fileName = set.Path.CombinePath(id).GetFullPath();
        return fileName;
    }

    [HttpPut]
    public async Task<Object> Put(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var fileName = GetFile(id);

        // 保存文件
        fileName.EnsureDirectory(true);

        var ms = Request.Body;
        using var fs = new FileStream(fileName, FileMode.OpenOrCreate);
        await ms.CopyToAsync(fs);
        fs.SetLength(fs.Length);

        var fi = fileName.AsFile();
        return new { name = id, length = fi.Length, time = fi.LastWriteTime, IsDirectory = false };
    }

    [HttpGet]
    public IActionResult Get(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var fileName = GetFile(id);
        var fi = fileName.AsFile();
        if (!fi.Exists) throw new Exception("文件不存在");

        return File(fi.ReadBytes(), "application/octet-stream");
    }

    [HttpGet]
    public String GetUrl(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var fileName = GetFile(id);
        var fi = fileName.AsFile();
        if (!fi.Exists) throw new Exception("文件不存在");

        //todo 实现计算Url
        throw new NotImplementedException();
    }

    /// <summary>搜索文件</summary>
    /// <param name="pattern">匹配模式。如/202304/*.jpg</param>
    /// <param name="start">开始序号。0开始</param>
    /// <param name="count">最大个数</param>
    /// <returns></returns>
    [HttpGet]
    public virtual IList<Object> Search(String pattern, Int32 start, Int32 count)
    {
        //if (searchPattern.IsNullOrEmpty()) throw new ArgumentNullException(nameof(searchPattern));

        var dir = "";
        var pt = "";
        if (!pattern.IsNullOrEmpty())
        {
            var p = pattern.LastIndexOfAny(new[] { '/', '\\' });
            if (p >= 0 && pattern.Substring(p + 1).Contains("*"))
            {
                dir = pattern.Substring(0, p);
                pt = pattern.Substring(p + 1);
            }
            else
            {
                dir = pattern;
            }
        }

        var di = _storageOptions.Path.CombinePath(dir).AsDirectory();
        if (!di.Exists) return null;

        var fis = di.GetFiles(pt).Skip(start).Take(count).ToArray();

        return fis.Select(e => new { name = e.Name, time = e.LastWriteTime }).Cast<Object>().ToList();
    }
}