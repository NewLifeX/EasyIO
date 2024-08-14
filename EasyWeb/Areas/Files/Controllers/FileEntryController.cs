using System.ComponentModel;
using EasyWeb.Data;
using EasyWeb.Models;
using EasyWeb.Services;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Log;
using NewLife.Web;
using Stardust;
using XCode.Membership;

namespace EasyWeb.Areas.Files.Controllers;

/// <summary>文件清单。详细记录每个文件源内的文件与目录信息</summary>
[Menu(20, true, Icon = "fa-table")]
[FilesArea]
public class FileEntryController : EntityController<FileEntry>
{
    static FileEntryController()
    {
        //LogOnChange = true;

        ListFields.RemoveField("SourceId", "SourceName", "Path", "Title", "FullName", "ParentId", "Hash", "RawUrl", "LastAccess");
        ListFields.RemoveField("LinkTarget", "LinkRedirect", "LastDownload");
        ListFields.RemoveCreateField().RemoveRemarkField();

        //{
        //    var df = ListFields.GetField("Code") as ListField;
        //    df.Url = "?code={Code}";
        //    df.Target = "_blank";
        //}
        {
            var df = ListFields.AddListField("files", null, "ParentName");
            df.DisplayName = "查看文件";
            df.Url = "/Files/FileEntry?parentId={Id}";
            df.DataVisible = e => (e as FileEntry).IsDirectory;
        }
        //{
        //    var df = ListFields.GetField("Kind") as ListField;
        //    df.GetValue = e => ((Int32)(e as FileEntry).Kind).ToString("X4");
        //}
        ListFields.TraceUrl("TraceId");
    }

    private readonly EntryService _entryService;

    public FileEntryController(EntryService entryService)
    {
        _entryService = entryService;
    }

    /// <summary>高级搜索。列表页查询、导出Excel、导出Json、分享页等使用</summary>
    /// <param name="p">分页器。包含分页排序参数，以及Http请求参数</param>
    /// <returns></returns>
    protected override IEnumerable<FileEntry> Search(Pager p)
    {
        var id = p["id"].ToInt(-1);
        if (id > 0)
        {
            var entity = FileEntry.FindById(id);
            if (entity != null) return [entity];
        }

        var storageId = p["storageId"].ToInt(-1);
        var parentId = p["parentId"].ToInt(-1);
        var isDir = p["isDir"]?.ToBoolean();
        var enable = p["enable"]?.ToBoolean();

        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return FileEntry.Search(storageId, null, parentId, isDir, enable, start, end, p["Q"], p);
    }

    [EntityAuthorize(PermissionFlags.Update)]
    public ActionResult SetRawRedirect(RedirectModes redirectMode)
    {
        if (GetRequest("keys") == null) throw new ArgumentNullException(nameof(SelectKeys));

        var rs = 0;
        foreach (var item in SelectKeys)
        {
            var entry = FileEntry.FindById(item.ToInt());
            if (entry != null)
            {
                entry.RedirectMode = redirectMode;
                rs += entry.Update();
            }
        }

        return JsonRefresh($"操作成功{rs}个");
    }

    [EntityAuthorize(PermissionFlags.Delete)]
    public ActionResult ClearFiles()
    {
        if (GetRequest("keys") == null) throw new ArgumentNullException(nameof(SelectKeys));

        var rs = 0;
        foreach (var item in SelectKeys)
        {
            var entry = FileEntry.FindById(item.ToInt());
            if (entry != null)
            {
                rs += _entryService.ClearFiles(entry, false);
            }
        }

        return JsonRefresh($"操作成功{rs}个");
    }
}