using EasyWeb.Data;
using EasyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Web;
using XCode.Membership;

namespace EasyWeb.Areas.Files.Controllers;

/// <summary>文件源。文件来源，定时从文件源抓取文件回到本地缓存目录，例如get.dot.net</summary>
[Menu(10, true, Icon = "fa-table")]
[FilesArea]
public class FileSourceController : EntityController<FileSource>
{
    static FileSourceController()
    {
        //LogOnChange = true;

        //ListFields.RemoveField("Id", "Creator");
        ListFields.RemoveCreateField().RemoveRemarkField();

        //{
        //    var df = ListFields.GetField("Code") as ListField;
        //    df.Url = "?code={Code}";
        //    df.Target = "_blank";
        //}
        //{
        //    var df = ListFields.AddListField("devices", null, "Onlines");
        //    df.DisplayName = "查看设备";
        //    df.Url = "Device?groupId={Id}";
        //    df.DataVisible = e => (e as FileSource).Devices > 0;
        //    df.Target = "_frame";
        //}
        //{
        //    var df = ListFields.GetField("Kind") as ListField;
        //    df.GetValue = e => ((Int32)(e as FileSource).Kind).ToString("X4");
        //}
        //ListFields.TraceUrl("TraceId");
    }

    private readonly ScanSourceService _sourceService;

    public FileSourceController(ScanSourceService sourceService) => _sourceService = sourceService;

    /// <summary>高级搜索。列表页查询、导出Excel、导出Json、分享页等使用</summary>
    /// <param name="p">分页器。包含分页排序参数，以及Http请求参数</param>
    /// <returns></returns>
    protected override IEnumerable<FileSource> Search(Pager p)
    {
        //var deviceId = p["deviceId"].ToInt(-1);
        //var enable = p["enable"]?.ToBoolean();

        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return FileSource.Search(start, end, p["Q"], p);
    }

    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult Scan()
    {
        var count = 0;
        var sid = 0;
        foreach (var key in SelectKeys)
        {
            var source = FileSource.FindById(key.ToInt());
            if (source != null)
            {
                if (source.Kind.EqualIgnoreCase("dotNet"))
                {
                    _ = Task.Run(() => _sourceService.ScanDotNet(source));
                    if (source.StorageId > 0) sid = source.StorageId;
                    count++;
                }
            }
        }

        if (sid > 0)
            _ = Task.Run(() => _sourceService.CreateLinks(sid));

        return JsonRefresh($"共处理[{count}]条", 1);
    }
}