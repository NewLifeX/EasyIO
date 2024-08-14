using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace EasyWeb.Data;

/// <summary>文件清单。详细记录每个文件源内的文件与目录信息</summary>
[Serializable]
[DataObject]
[Description("文件清单。详细记录每个文件源内的文件与目录信息")]
[BindIndex("IU_FileEntry_StorageId_ParentId_Name", true, "StorageId,ParentId,Name")]
[BindIndex("IX_FileEntry_ParentId", false, "ParentId")]
[BindIndex("IX_FileEntry_SourceId_ParentId", false, "SourceId,ParentId")]
[BindIndex("IX_FileEntry_StorageId_Path", false, "StorageId,Path")]
[BindTable("FileEntry", Description = "文件清单。详细记录每个文件源内的文件与目录信息", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class FileEntry
{
    #region 属性
    private Int32 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("Id", "编号", "")]
    public Int32 Id { get => _Id; set { if (OnPropertyChanging("Id", value)) { _Id = value; OnPropertyChanged("Id"); } } }

    private Int32 _StorageId;
    /// <summary>仓库</summary>
    [DisplayName("仓库")]
    [Description("仓库")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("StorageId", "仓库", "")]
    public Int32 StorageId { get => _StorageId; set { if (OnPropertyChanging("StorageId", value)) { _StorageId = value; OnPropertyChanged("StorageId"); } } }

    private Int32 _SourceId;
    /// <summary>来源</summary>
    [DisplayName("来源")]
    [Description("来源")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("SourceId", "来源", "")]
    public Int32 SourceId { get => _SourceId; set { if (OnPropertyChanging("SourceId", value)) { _SourceId = value; OnPropertyChanged("SourceId"); } } }

    private String _Name;
    /// <summary>名称</summary>
    [DisplayName("名称")]
    [Description("名称")]
    [DataObjectField(false, false, true, 250)]
    [BindColumn("Name", "名称", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String _Title;
    /// <summary>标题</summary>
    [DisplayName("标题")]
    [Description("标题")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Title", "标题", "")]
    public String Title { get => _Title; set { if (OnPropertyChanging("Title", value)) { _Title = value; OnPropertyChanged("Title"); } } }

    private String _FullName;
    /// <summary>全路径。文件保存在本地的相对路径</summary>
    [DisplayName("全路径")]
    [Description("全路径。文件保存在本地的相对路径")]
    [DataObjectField(false, false, true, 250)]
    [BindColumn("FullName", "全路径。文件保存在本地的相对路径", "")]
    public String FullName { get => _FullName; set { if (OnPropertyChanging("FullName", value)) { _FullName = value; OnPropertyChanged("FullName"); } } }

    private String _Path;
    /// <summary>路径。用于Url的相对路径</summary>
    [DisplayName("路径")]
    [Description("路径。用于Url的相对路径")]
    [DataObjectField(false, false, true, 250)]
    [BindColumn("Path", "路径。用于Url的相对路径", "")]
    public String Path { get => _Path; set { if (OnPropertyChanging("Path", value)) { _Path = value; OnPropertyChanged("Path"); } } }

    private Boolean _Enable;
    /// <summary>启用</summary>
    [DisplayName("启用")]
    [Description("启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private Int32 _ParentId;
    /// <summary>上级目录。0表示顶级</summary>
    [DisplayName("上级目录")]
    [Description("上级目录。0表示顶级")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ParentId", "上级目录。0表示顶级", "")]
    public Int32 ParentId { get => _ParentId; set { if (OnPropertyChanging("ParentId", value)) { _ParentId = value; OnPropertyChanged("ParentId"); } } }

    private Boolean _IsDirectory;
    /// <summary>目录。当前清单是目录而不是文件</summary>
    [DisplayName("目录")]
    [Description("目录。当前清单是目录而不是文件")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("IsDirectory", "目录。当前清单是目录而不是文件", "")]
    public Boolean IsDirectory { get => _IsDirectory; set { if (OnPropertyChanging("IsDirectory", value)) { _IsDirectory = value; OnPropertyChanged("IsDirectory"); } } }

    private Int64 _Size;
    /// <summary>大小。文件大小</summary>
    [DisplayName("大小")]
    [Description("大小。文件大小")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Size", "大小。文件大小", "", ItemType = "GMK")]
    public Int64 Size { get => _Size; set { if (OnPropertyChanging("Size", value)) { _Size = value; OnPropertyChanged("Size"); } } }

    private Int32 _Version;
    /// <summary>版本。用于排序</summary>
    [DisplayName("版本")]
    [Description("版本。用于排序")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Version", "版本。用于排序", "")]
    public Int32 Version { get => _Version; set { if (OnPropertyChanging("Version", value)) { _Version = value; OnPropertyChanged("Version"); } } }

    private String _Tag;
    /// <summary>标签。类型标签</summary>
    [DisplayName("标签")]
    [Description("标签。类型标签")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Tag", "标签。类型标签", "")]
    public String Tag { get => _Tag; set { if (OnPropertyChanging("Tag", value)) { _Tag = value; OnPropertyChanged("Tag"); } } }

    private DateTime _LastWrite;
    /// <summary>最后修改</summary>
    [DisplayName("最后修改")]
    [Description("最后修改")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastWrite", "最后修改", "")]
    public DateTime LastWrite { get => _LastWrite; set { if (OnPropertyChanging("LastWrite", value)) { _LastWrite = value; OnPropertyChanged("LastWrite"); } } }

    private DateTime _LastAccess;
    /// <summary>最后访问</summary>
    [DisplayName("最后访问")]
    [Description("最后访问")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastAccess", "最后访问", "")]
    public DateTime LastAccess { get => _LastAccess; set { if (OnPropertyChanging("LastAccess", value)) { _LastAccess = value; OnPropertyChanged("LastAccess"); } } }

    private DateTime _LastScan;
    /// <summary>最后扫描。最后一次扫描访问的时间</summary>
    [DisplayName("最后扫描")]
    [Description("最后扫描。最后一次扫描访问的时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastScan", "最后扫描。最后一次扫描访问的时间", "")]
    public DateTime LastScan { get => _LastScan; set { if (OnPropertyChanging("LastScan", value)) { _LastScan = value; OnPropertyChanged("LastScan"); } } }

    private String _Hash;
    /// <summary>哈希。MD5哈希或SHA512</summary>
    [DisplayName("哈希")]
    [Description("哈希。MD5哈希或SHA512")]
    [DataObjectField(false, false, true, 600)]
    [BindColumn("Hash", "哈希。MD5哈希或SHA512", "")]
    public String Hash { get => _Hash; set { if (OnPropertyChanging("Hash", value)) { _Hash = value; OnPropertyChanged("Hash"); } } }

    private String _RawUrl;
    /// <summary>原始地址。文件的原始地址，如果文件在本地不存在时，跳转原始地址</summary>
    [DisplayName("原始地址")]
    [Description("原始地址。文件的原始地址，如果文件在本地不存在时，跳转原始地址")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("RawUrl", "原始地址。文件的原始地址，如果文件在本地不存在时，跳转原始地址", "")]
    public String RawUrl { get => _RawUrl; set { if (OnPropertyChanging("RawUrl", value)) { _RawUrl = value; OnPropertyChanged("RawUrl"); } } }

    private EasyWeb.Models.RedirectModes _RedirectMode;
    /// <summary>原始跳转。跳转到原始地址</summary>
    [DisplayName("原始跳转")]
    [Description("原始跳转。跳转到原始地址")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("RedirectMode", "原始跳转。跳转到原始地址", "")]
    public EasyWeb.Models.RedirectModes RedirectMode { get => _RedirectMode; set { if (OnPropertyChanging("RedirectMode", value)) { _RedirectMode = value; OnPropertyChanged("RedirectMode"); } } }

    private String _LinkTarget;
    /// <summary>链接目标。链接到目标文件，支持*和!*匹配目标目录的最新匹配文件</summary>
    [DisplayName("链接目标")]
    [Description("链接目标。链接到目标文件，支持*和!*匹配目标目录的最新匹配文件")]
    [DataObjectField(false, false, true, 250)]
    [BindColumn("LinkTarget", "链接目标。链接到目标文件，支持*和!*匹配目标目录的最新匹配文件", "")]
    public String LinkTarget { get => _LinkTarget; set { if (OnPropertyChanging("LinkTarget", value)) { _LinkTarget = value; OnPropertyChanged("LinkTarget"); } } }

    private Boolean _LinkRedirect;
    /// <summary>链接跳转。跳转到目标文件</summary>
    [DisplayName("链接跳转")]
    [Description("链接跳转。跳转到目标文件")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("LinkRedirect", "链接跳转。跳转到目标文件", "")]
    public Boolean LinkRedirect { get => _LinkRedirect; set { if (OnPropertyChanging("LinkRedirect", value)) { _LinkRedirect = value; OnPropertyChanged("LinkRedirect"); } } }

    private Int32 _Times;
    /// <summary>次数。下载次数</summary>
    [DisplayName("次数")]
    [Description("次数。下载次数")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Times", "次数。下载次数", "")]
    public Int32 Times { get => _Times; set { if (OnPropertyChanging("Times", value)) { _Times = value; OnPropertyChanged("Times"); } } }

    private DateTime _LastDownload;
    /// <summary>最后下载</summary>
    [DisplayName("最后下载")]
    [Description("最后下载")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastDownload", "最后下载", "")]
    public DateTime LastDownload { get => _LastDownload; set { if (OnPropertyChanging("LastDownload", value)) { _LastDownload = value; OnPropertyChanged("LastDownload"); } } }

    private String _TraceId;
    /// <summary>链路。链路追踪</summary>
    [DisplayName("链路")]
    [Description("链路。链路追踪")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("TraceId", "链路。链路追踪", "")]
    public String TraceId { get => _TraceId; set { if (OnPropertyChanging("TraceId", value)) { _TraceId = value; OnPropertyChanged("TraceId"); } } }

    private Int32 _CreateUserId;
    /// <summary>创建者</summary>
    [Category("扩展")]
    [DisplayName("创建者")]
    [Description("创建者")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CreateUserId", "创建者", "")]
    public Int32 CreateUserId { get => _CreateUserId; set { if (OnPropertyChanging("CreateUserId", value)) { _CreateUserId = value; OnPropertyChanged("CreateUserId"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [Category("扩展")]
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private String _CreateIP;
    /// <summary>创建地址</summary>
    [Category("扩展")]
    [DisplayName("创建地址")]
    [Description("创建地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("CreateIP", "创建地址", "")]
    public String CreateIP { get => _CreateIP; set { if (OnPropertyChanging("CreateIP", value)) { _CreateIP = value; OnPropertyChanged("CreateIP"); } } }

    private Int32 _UpdateUserId;
    /// <summary>更新者</summary>
    [Category("扩展")]
    [DisplayName("更新者")]
    [Description("更新者")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("UpdateUserId", "更新者", "")]
    public Int32 UpdateUserId { get => _UpdateUserId; set { if (OnPropertyChanging("UpdateUserId", value)) { _UpdateUserId = value; OnPropertyChanged("UpdateUserId"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [Category("扩展")]
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }

    private String _UpdateIP;
    /// <summary>更新地址</summary>
    [Category("扩展")]
    [DisplayName("更新地址")]
    [Description("更新地址")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("UpdateIP", "更新地址", "")]
    public String UpdateIP { get => _UpdateIP; set { if (OnPropertyChanging("UpdateIP", value)) { _UpdateIP = value; OnPropertyChanged("UpdateIP"); } } }

    private String _Remark;
    /// <summary>备注</summary>
    [Category("扩展")]
    [DisplayName("备注")]
    [Description("备注")]
    [DataObjectField(false, false, true, 500)]
    [BindColumn("Remark", "备注", "")]
    public String Remark { get => _Remark; set { if (OnPropertyChanging("Remark", value)) { _Remark = value; OnPropertyChanged("Remark"); } } }
    #endregion

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object this[String name]
    {
        get => name switch
        {
            "Id" => _Id,
            "StorageId" => _StorageId,
            "SourceId" => _SourceId,
            "Name" => _Name,
            "Title" => _Title,
            "FullName" => _FullName,
            "Path" => _Path,
            "Enable" => _Enable,
            "ParentId" => _ParentId,
            "IsDirectory" => _IsDirectory,
            "Size" => _Size,
            "Version" => _Version,
            "Tag" => _Tag,
            "LastWrite" => _LastWrite,
            "LastAccess" => _LastAccess,
            "LastScan" => _LastScan,
            "Hash" => _Hash,
            "RawUrl" => _RawUrl,
            "RedirectMode" => _RedirectMode,
            "LinkTarget" => _LinkTarget,
            "LinkRedirect" => _LinkRedirect,
            "Times" => _Times,
            "LastDownload" => _LastDownload,
            "TraceId" => _TraceId,
            "CreateUserId" => _CreateUserId,
            "CreateTime" => _CreateTime,
            "CreateIP" => _CreateIP,
            "UpdateUserId" => _UpdateUserId,
            "UpdateTime" => _UpdateTime,
            "UpdateIP" => _UpdateIP,
            "Remark" => _Remark,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "Id": _Id = value.ToInt(); break;
                case "StorageId": _StorageId = value.ToInt(); break;
                case "SourceId": _SourceId = value.ToInt(); break;
                case "Name": _Name = Convert.ToString(value); break;
                case "Title": _Title = Convert.ToString(value); break;
                case "FullName": _FullName = Convert.ToString(value); break;
                case "Path": _Path = Convert.ToString(value); break;
                case "Enable": _Enable = value.ToBoolean(); break;
                case "ParentId": _ParentId = value.ToInt(); break;
                case "IsDirectory": _IsDirectory = value.ToBoolean(); break;
                case "Size": _Size = value.ToLong(); break;
                case "Version": _Version = value.ToInt(); break;
                case "Tag": _Tag = Convert.ToString(value); break;
                case "LastWrite": _LastWrite = value.ToDateTime(); break;
                case "LastAccess": _LastAccess = value.ToDateTime(); break;
                case "LastScan": _LastScan = value.ToDateTime(); break;
                case "Hash": _Hash = Convert.ToString(value); break;
                case "RawUrl": _RawUrl = Convert.ToString(value); break;
                case "RedirectMode": _RedirectMode = (EasyWeb.Models.RedirectModes)value.ToInt(); break;
                case "LinkTarget": _LinkTarget = Convert.ToString(value); break;
                case "LinkRedirect": _LinkRedirect = value.ToBoolean(); break;
                case "Times": _Times = value.ToInt(); break;
                case "LastDownload": _LastDownload = value.ToDateTime(); break;
                case "TraceId": _TraceId = Convert.ToString(value); break;
                case "CreateUserId": _CreateUserId = value.ToInt(); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "CreateIP": _CreateIP = Convert.ToString(value); break;
                case "UpdateUserId": _UpdateUserId = value.ToInt(); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                case "UpdateIP": _UpdateIP = Convert.ToString(value); break;
                case "Remark": _Remark = Convert.ToString(value); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 关联映射
    /// <summary>仓库</summary>
    [XmlIgnore, IgnoreDataMember, ScriptIgnore]
    public FileStorage Storage => Extends.Get(nameof(Storage), k => FileStorage.FindById(StorageId));

    /// <summary>仓库</summary>
    [Map(nameof(StorageId), typeof(FileStorage), "Id")]
    public String StorageName => Storage?.Name;

    /// <summary>来源</summary>
    [XmlIgnore, IgnoreDataMember, ScriptIgnore]
    public FileSource Source => Extends.Get(nameof(Source), k => FileSource.FindById(SourceId));

    /// <summary>来源</summary>
    [Map(nameof(SourceId), typeof(FileSource), "Id")]
    public String SourceName => Source?.Name;

    /// <summary>上级目录</summary>
    [XmlIgnore, IgnoreDataMember, ScriptIgnore]
    public FileEntry Parent => Extends.Get(nameof(Parent), k => FileEntry.FindById(ParentId));

    /// <summary>上级目录</summary>
    [Map(nameof(ParentId), typeof(FileEntry), "Id")]
    public String ParentName => Parent?.ToString();

    #endregion

    #region 扩展查询
    /// <summary>根据仓库查找</summary>
    /// <param name="storageId">仓库</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByStorageId(Int32 storageId)
    {
        if (storageId < 0) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.StorageId == storageId);

        return FindAll(_.StorageId == storageId);
    }
    #endregion

    #region 字段名
    /// <summary>取得文件清单字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>仓库</summary>
        public static readonly Field StorageId = FindByName("StorageId");

        /// <summary>来源</summary>
        public static readonly Field SourceId = FindByName("SourceId");

        /// <summary>名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>标题</summary>
        public static readonly Field Title = FindByName("Title");

        /// <summary>全路径。文件保存在本地的相对路径</summary>
        public static readonly Field FullName = FindByName("FullName");

        /// <summary>路径。用于Url的相对路径</summary>
        public static readonly Field Path = FindByName("Path");

        /// <summary>启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>上级目录。0表示顶级</summary>
        public static readonly Field ParentId = FindByName("ParentId");

        /// <summary>目录。当前清单是目录而不是文件</summary>
        public static readonly Field IsDirectory = FindByName("IsDirectory");

        /// <summary>大小。文件大小</summary>
        public static readonly Field Size = FindByName("Size");

        /// <summary>版本。用于排序</summary>
        public static readonly Field Version = FindByName("Version");

        /// <summary>标签。类型标签</summary>
        public static readonly Field Tag = FindByName("Tag");

        /// <summary>最后修改</summary>
        public static readonly Field LastWrite = FindByName("LastWrite");

        /// <summary>最后访问</summary>
        public static readonly Field LastAccess = FindByName("LastAccess");

        /// <summary>最后扫描。最后一次扫描访问的时间</summary>
        public static readonly Field LastScan = FindByName("LastScan");

        /// <summary>哈希。MD5哈希或SHA512</summary>
        public static readonly Field Hash = FindByName("Hash");

        /// <summary>原始地址。文件的原始地址，如果文件在本地不存在时，跳转原始地址</summary>
        public static readonly Field RawUrl = FindByName("RawUrl");

        /// <summary>原始跳转。跳转到原始地址</summary>
        public static readonly Field RedirectMode = FindByName("RedirectMode");

        /// <summary>链接目标。链接到目标文件，支持*和!*匹配目标目录的最新匹配文件</summary>
        public static readonly Field LinkTarget = FindByName("LinkTarget");

        /// <summary>链接跳转。跳转到目标文件</summary>
        public static readonly Field LinkRedirect = FindByName("LinkRedirect");

        /// <summary>次数。下载次数</summary>
        public static readonly Field Times = FindByName("Times");

        /// <summary>最后下载</summary>
        public static readonly Field LastDownload = FindByName("LastDownload");

        /// <summary>链路。链路追踪</summary>
        public static readonly Field TraceId = FindByName("TraceId");

        /// <summary>创建者</summary>
        public static readonly Field CreateUserId = FindByName("CreateUserId");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>创建地址</summary>
        public static readonly Field CreateIP = FindByName("CreateIP");

        /// <summary>更新者</summary>
        public static readonly Field UpdateUserId = FindByName("UpdateUserId");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        /// <summary>更新地址</summary>
        public static readonly Field UpdateIP = FindByName("UpdateIP");

        /// <summary>备注</summary>
        public static readonly Field Remark = FindByName("Remark");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得文件清单字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>仓库</summary>
        public const String StorageId = "StorageId";

        /// <summary>来源</summary>
        public const String SourceId = "SourceId";

        /// <summary>名称</summary>
        public const String Name = "Name";

        /// <summary>标题</summary>
        public const String Title = "Title";

        /// <summary>全路径。文件保存在本地的相对路径</summary>
        public const String FullName = "FullName";

        /// <summary>路径。用于Url的相对路径</summary>
        public const String Path = "Path";

        /// <summary>启用</summary>
        public const String Enable = "Enable";

        /// <summary>上级目录。0表示顶级</summary>
        public const String ParentId = "ParentId";

        /// <summary>目录。当前清单是目录而不是文件</summary>
        public const String IsDirectory = "IsDirectory";

        /// <summary>大小。文件大小</summary>
        public const String Size = "Size";

        /// <summary>版本。用于排序</summary>
        public const String Version = "Version";

        /// <summary>标签。类型标签</summary>
        public const String Tag = "Tag";

        /// <summary>最后修改</summary>
        public const String LastWrite = "LastWrite";

        /// <summary>最后访问</summary>
        public const String LastAccess = "LastAccess";

        /// <summary>最后扫描。最后一次扫描访问的时间</summary>
        public const String LastScan = "LastScan";

        /// <summary>哈希。MD5哈希或SHA512</summary>
        public const String Hash = "Hash";

        /// <summary>原始地址。文件的原始地址，如果文件在本地不存在时，跳转原始地址</summary>
        public const String RawUrl = "RawUrl";

        /// <summary>原始跳转。跳转到原始地址</summary>
        public const String RedirectMode = "RedirectMode";

        /// <summary>链接目标。链接到目标文件，支持*和!*匹配目标目录的最新匹配文件</summary>
        public const String LinkTarget = "LinkTarget";

        /// <summary>链接跳转。跳转到目标文件</summary>
        public const String LinkRedirect = "LinkRedirect";

        /// <summary>次数。下载次数</summary>
        public const String Times = "Times";

        /// <summary>最后下载</summary>
        public const String LastDownload = "LastDownload";

        /// <summary>链路。链路追踪</summary>
        public const String TraceId = "TraceId";

        /// <summary>创建者</summary>
        public const String CreateUserId = "CreateUserId";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>创建地址</summary>
        public const String CreateIP = "CreateIP";

        /// <summary>更新者</summary>
        public const String UpdateUserId = "UpdateUserId";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";

        /// <summary>更新地址</summary>
        public const String UpdateIP = "UpdateIP";

        /// <summary>备注</summary>
        public const String Remark = "Remark";
    }
    #endregion
}
