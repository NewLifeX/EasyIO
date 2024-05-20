﻿using Microsoft.AspNetCore.Mvc;
using EasyWeb.Data;
using NewLife;
using NewLife.Cube;
using NewLife.Cube.Extensions;
using NewLife.Cube.ViewModels;
using NewLife.Log;
using NewLife.Web;
using XCode.Membership;
using static EasyWeb.Data.FileEntry;

namespace EasyWeb.Areas.Files.Controllers;

/// <summary>文件清单。详细记录每个文件源内的文件与目录信息</summary>
[Menu(20, true, Icon = "fa-table")]
[FilesArea]
public class FileEntryController : EntityController<FileEntry>
{
    static FileEntryController()
    {
        //LogOnChange = true;

        ListFields.RemoveField("SourceId", "Title", "FullName", "ParentId", "Hash", "RawUrl");
        ListFields.RemoveCreateField().RemoveRemarkField();

        //{
        //    var df = ListFields.GetField("Code") as ListField;
        //    df.Url = "?code={Code}";
        //    df.Target = "_blank";
        //}
        {
            var df = ListFields.AddListField("files", "Enable");
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

    //private readonly ITracer _tracer;

    //public FileEntryController(ITracer tracer)
    //{
    //    _tracer = tracer;
    //}

    /// <summary>高级搜索。列表页查询、导出Excel、导出Json、分享页等使用</summary>
    /// <param name="p">分页器。包含分页排序参数，以及Http请求参数</param>
    /// <returns></returns>
    protected override IEnumerable<FileEntry> Search(Pager p)
    {
        var storageId = p["storageId"].ToInt(-1);
        var parentId = p["parentId"].ToInt(-1);
        //var enable = p["enable"]?.ToBoolean();

        var start = p["dtStart"].ToDateTime();
        var end = p["dtEnd"].ToDateTime();

        return FileEntry.Search(storageId, null, parentId, start, end, p["Q"], p);
    }
}