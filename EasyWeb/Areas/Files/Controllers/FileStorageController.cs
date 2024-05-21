using Microsoft.AspNetCore.Mvc;
using EasyWeb.Data;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Log;
using NewLife.Web;
using XCode.Membership;
using EasyWeb.Services;

namespace EasyWeb.Areas.Files.Controllers;

/// <summary>文件仓库。文件存储的根目录</summary>
[Menu(30, true, Icon = "fa-table")]
[FilesArea]
public class FileStorageController : EntityController<FileStorage>
{
    private readonly ScanStorageService _storageService;

    static FileStorageController()
    {
        //LogOnChange = true;

        //ListFields.RemoveField("Id", "Creator");
        ListFields.RemoveCreateField().RemoveRemarkField();

        //{
        //    var df = ListFields.GetField("Code") as ListField;
        //    df.Url = "?code={Code}";
        //    df.Target = "_blank";
        //}
        {
            var df = ListFields.AddListField("details", null, "HomeDirectory");
            df.DisplayName = "查看文件";
            df.Url = "/Files/FileEntry?storageId={Id}&parentId=0";
        }
        {
            var df = ListFields.AddListField("details", null, "Period");
            df.DisplayName = "立即扫描";
            df.Url = "/Files/FileStorage/Scan?keys={Id}";
            df.DataAction = "action";
        }
    }

    public FileStorageController(ScanStorageService storageService) => _storageService = storageService;

    /// <summary>高级搜索。列表页查询、导出Excel、导出Json、分享页等使用</summary>
    /// <param name="p">分页器。包含分页排序参数，以及Http请求参数</param>
    /// <returns></returns>
    protected override IEnumerable<FileStorage> Search(Pager p)
    {
        //var deviceId = p["deviceId"].ToInt(-1);
        //var enable = p["enable"]?.ToBoolean();

        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return FileStorage.Search(start, end, p["Q"], p);
    }

    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult Scan()
    {
        var count = 0;
        foreach (var key in SelectKeys)
        {
            var storage = FileStorage.FindById(key.ToInt());
            if (storage != null)
            {
                _ = Task.Run(() => _storageService.Scan(storage, null, null, 1));
                count++;
            }
        }

        return JsonRefresh($"共处理[{count}]条", 1);
    }
}