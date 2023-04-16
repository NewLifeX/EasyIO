using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.EasyIO.Options;

namespace NewLife.EasyWeb.Controllers;

/// <summary>文件控制器</summary>
[ApiController]
[Route("[controller]/[action]")]
public class IOController : ApiControllerBase
{
    private readonly FileStorageOptions _storageOptions;

    /// <summary>实例化</summary>
    /// <param name="storageOptions"></param>
    public IOController(FileStorageOptions storageOptions) => _storageOptions = storageOptions;

    private String GetPath(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var set = _storageOptions;
        if (set.Path.IsNullOrEmpty()) throw new Exception("未配置存储信息");

        return set.Path.CombinePath(id).GetFullPath();
    }

    /// <summary>上传文件对象</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [HttpPut]
    public async Task<Object> Put(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var fileName = GetPath(id);

        // 保存文件
        fileName.EnsureDirectory(true);

        var ms = Request.Body;
        using var fs = new FileStream(fileName, FileMode.OpenOrCreate);
        await ms.CopyToAsync(fs);
        fs.SetLength(fs.Length);

        var fi = fileName.AsFile();
        return new { name = id, length = fi.Length, time = fi.LastWriteTime, IsDirectory = false };
    }

    /// <summary>获取文件对象内容</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet]
    public IActionResult Get(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var fileName = GetPath(id);
        var fi = fileName.AsFile();
        if (!fi.Exists) throw new Exception("文件不存在");

        return File(fi.ReadBytes(), "application/octet-stream");
    }

    /// <summary>获取文件对象的访问Url</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet]
    public String GetUrl(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var fileName = GetPath(id);
        var fi = fileName.AsFile();
        if (!fi.Exists) throw new Exception("文件不存在");

        //todo 实现计算Url
        throw new NotImplementedException();
    }

    /// <summary>删除文件对象</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpDelete]
    public Int32 Delete(String id)
    {
        if (id.IsNullOrEmpty()) throw new Exception("找不到记录！id=" + id);

        var path = GetPath(id);
        if (path.EndsWith('/') || path.EndsWith('\\'))
        {
            var di = path.AsDirectory();
            if (!di.Exists) return 0;

            di.Delete();

            return 1;
        }
        else
        {
            var fi = path.AsFile();
            if (!fi.Exists) return 0;

            fi.Delete();

            return 1;
        }
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
        // 强制count默认值为100
        if (count <= 0) count = 100;
        if (start <= 0) start = 0;

        var dir = "";
        var pt = "*";
        if (!pattern.IsNullOrEmpty())
        {
            var p = pattern.LastIndexOfAny(new[] { '/', '\\' });
            if (p >= 0 && pattern[(p + 1)..].Contains('*'))
            {
                dir = pattern[..p];
                pt = pattern[(p + 1)..];
            }
            else
            {
                dir = pattern;
            }
        }

        var di = _storageOptions.Path.CombinePath(dir).AsDirectory();
        if (!di.Exists) return null;

        var root = _storageOptions.Path.EnsureEnd("/").GetFullPath();
        var rs = new List<Object>();

        // 子目录列表
        var dis = di.GetDirectories(pt);
        if (dis.Length > 0)
        {
            foreach (var item in dis.Skip(start).Take(count))
            {
                rs.Add(new
                {
                    name = item.FullName.TrimStart(root).Replace('\\', '/'),
                    time = item.LastWriteTime
                });
            }
            start += dis.Length;
            count -= rs.Count;
        }
        if (count == 0) return rs;

        // 文件列表
        var fis = di.GetFiles(pt);
        if (fis.Length > 0)
        {
            foreach (var item in fis.Skip(start).Take(count))
            {
                rs.Add(new
                {
                    name = item.FullName.TrimStart(root).Replace('\\', '/'),
                    time = item.LastWriteTime
                });
            }
        }

        return rs;
    }
}