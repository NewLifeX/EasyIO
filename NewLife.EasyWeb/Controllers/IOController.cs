using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.EasyIO.Options;
using NewLife.Log;

namespace EasyIO.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class IOController : ControllerBase
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

    [HttpGet]
    public async Task<String> Put(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var fileName = GetFile(id);

        using var span = _tracer?.NewSpan("UploadFile", fileName);
        XTrace.WriteLine("上传：{0}", fileName);

        // 保存文件
        try
        {
            fileName.EnsureDirectory(true);

            var ms = Request.Body;
            using var fs = new FileStream(fileName, FileMode.OpenOrCreate);
            await ms.CopyToAsync(fs);
            fs.SetLength(fs.Length);

            return id;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            XTrace.WriteException(ex);
            throw;
        }
    }

    [HttpGet]
    public Byte[] Get(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var fileName = GetFile(id);
        var fi = fileName.AsFile();
        if (!fi.Exists) throw new Exception("文件不存在");

        return fi.ReadBytes();
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
}