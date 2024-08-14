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

/// <summary>文件源。文件来源，定时从文件源抓取文件回到本地缓存目录，例如get.dot.net</summary>
[Serializable]
[DataObject]
[Description("文件源。文件来源，定时从文件源抓取文件回到本地缓存目录，例如get.dot.net")]
[BindIndex("IU_FileSource_Name", true, "Name")]
[BindTable("FileSource", Description = "文件源。文件来源，定时从文件源抓取文件回到本地缓存目录，例如get.dot.net", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class FileSource
{
    #region 属性
    private Int32 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("Id", "编号", "")]
    public Int32 Id { get => _Id; set { if (OnPropertyChanging("Id", value)) { _Id = value; OnPropertyChanged("Id"); } } }

    private String _Name;
    /// <summary>名称</summary>
    [DisplayName("名称")]
    [Description("名称")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Name", "名称", "", Master = true)]
    public String Name { get => _Name; set { if (OnPropertyChanging("Name", value)) { _Name = value; OnPropertyChanged("Name"); } } }

    private String _Kind;
    /// <summary>种类。如dotnet</summary>
    [DisplayName("种类")]
    [Description("种类。如dotnet")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Kind", "种类。如dotnet", "")]
    public String Kind { get => _Kind; set { if (OnPropertyChanging("Kind", value)) { _Kind = value; OnPropertyChanged("Kind"); } } }

    private String _Url;
    /// <summary>地址</summary>
    [DisplayName("地址")]
    [Description("地址")]
    [DataObjectField(false, false, true, 250)]
    [BindColumn("Url", "地址", "")]
    public String Url { get => _Url; set { if (OnPropertyChanging("Url", value)) { _Url = value; OnPropertyChanged("Url"); } } }

    private Boolean _Enable;
    /// <summary>启用</summary>
    [DisplayName("启用")]
    [Description("启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private Int32 _StorageId;
    /// <summary>仓库</summary>
    [DisplayName("仓库")]
    [Description("仓库")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("StorageId", "仓库", "")]
    public Int32 StorageId { get => _StorageId; set { if (OnPropertyChanging("StorageId", value)) { _StorageId = value; OnPropertyChanged("StorageId"); } } }

    private String _RootPath;
    /// <summary>主目录。存放在目标仓库的指定路径</summary>
    [DisplayName("主目录")]
    [Description("主目录。存放在目标仓库的指定路径")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("RootPath", "主目录。存放在目标仓库的指定路径", "")]
    public String RootPath { get => _RootPath; set { if (OnPropertyChanging("RootPath", value)) { _RootPath = value; OnPropertyChanged("RootPath"); } } }

    private String _Protocol;
    /// <summary>协议。http或https，留空表示用原来协议</summary>
    [DisplayName("协议")]
    [Description("协议。http或https，留空表示用原来协议")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Protocol", "协议。http或https，留空表示用原来协议", "")]
    public String Protocol { get => _Protocol; set { if (OnPropertyChanging("Protocol", value)) { _Protocol = value; OnPropertyChanged("Protocol"); } } }

    private Int32 _Period;
    /// <summary>同步周期。默认60秒</summary>
    [DisplayName("同步周期")]
    [Description("同步周期。默认60秒")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Period", "同步周期。默认60秒", "")]
    public Int32 Period { get => _Period; set { if (OnPropertyChanging("Period", value)) { _Period = value; OnPropertyChanged("Period"); } } }

    private String _Whites;
    /// <summary>白名单。仅搜索匹配的文件，支持*，多个规则逗号隔开</summary>
    [DisplayName("白名单")]
    [Description("白名单。仅搜索匹配的文件，支持*，多个规则逗号隔开")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Whites", "白名单。仅搜索匹配的文件，支持*，多个规则逗号隔开", "")]
    public String Whites { get => _Whites; set { if (OnPropertyChanging("Whites", value)) { _Whites = value; OnPropertyChanged("Whites"); } } }

    private String _Blacks;
    /// <summary>黑名单。仅搜索匹配以外的文件，支持*，多个规则逗号隔开</summary>
    [DisplayName("黑名单")]
    [Description("黑名单。仅搜索匹配以外的文件，支持*，多个规则逗号隔开")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Blacks", "黑名单。仅搜索匹配以外的文件，支持*，多个规则逗号隔开", "")]
    public String Blacks { get => _Blacks; set { if (OnPropertyChanging("Blacks", value)) { _Blacks = value; OnPropertyChanged("Blacks"); } } }

    private DateTime _LastScan;
    /// <summary>最后扫描。记录最后一次扫描时间</summary>
    [DisplayName("最后扫描")]
    [Description("最后扫描。记录最后一次扫描时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("LastScan", "最后扫描。记录最后一次扫描时间", "")]
    public DateTime LastScan { get => _LastScan; set { if (OnPropertyChanging("LastScan", value)) { _LastScan = value; OnPropertyChanged("LastScan"); } } }

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
            "Name" => _Name,
            "Kind" => _Kind,
            "Url" => _Url,
            "Enable" => _Enable,
            "StorageId" => _StorageId,
            "RootPath" => _RootPath,
            "Protocol" => _Protocol,
            "Period" => _Period,
            "Whites" => _Whites,
            "Blacks" => _Blacks,
            "LastScan" => _LastScan,
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
                case "Name": _Name = Convert.ToString(value); break;
                case "Kind": _Kind = Convert.ToString(value); break;
                case "Url": _Url = Convert.ToString(value); break;
                case "Enable": _Enable = value.ToBoolean(); break;
                case "StorageId": _StorageId = value.ToInt(); break;
                case "RootPath": _RootPath = Convert.ToString(value); break;
                case "Protocol": _Protocol = Convert.ToString(value); break;
                case "Period": _Period = value.ToInt(); break;
                case "Whites": _Whites = Convert.ToString(value); break;
                case "Blacks": _Blacks = Convert.ToString(value); break;
                case "LastScan": _LastScan = value.ToDateTime(); break;
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

    #endregion

    #region 扩展查询
    #endregion

    #region 字段名
    /// <summary>取得文件源字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>种类。如dotnet</summary>
        public static readonly Field Kind = FindByName("Kind");

        /// <summary>地址</summary>
        public static readonly Field Url = FindByName("Url");

        /// <summary>启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>仓库</summary>
        public static readonly Field StorageId = FindByName("StorageId");

        /// <summary>主目录。存放在目标仓库的指定路径</summary>
        public static readonly Field RootPath = FindByName("RootPath");

        /// <summary>协议。http或https，留空表示用原来协议</summary>
        public static readonly Field Protocol = FindByName("Protocol");

        /// <summary>同步周期。默认60秒</summary>
        public static readonly Field Period = FindByName("Period");

        /// <summary>白名单。仅搜索匹配的文件，支持*，多个规则逗号隔开</summary>
        public static readonly Field Whites = FindByName("Whites");

        /// <summary>黑名单。仅搜索匹配以外的文件，支持*，多个规则逗号隔开</summary>
        public static readonly Field Blacks = FindByName("Blacks");

        /// <summary>最后扫描。记录最后一次扫描时间</summary>
        public static readonly Field LastScan = FindByName("LastScan");

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

    /// <summary>取得文件源字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>名称</summary>
        public const String Name = "Name";

        /// <summary>种类。如dotnet</summary>
        public const String Kind = "Kind";

        /// <summary>地址</summary>
        public const String Url = "Url";

        /// <summary>启用</summary>
        public const String Enable = "Enable";

        /// <summary>仓库</summary>
        public const String StorageId = "StorageId";

        /// <summary>主目录。存放在目标仓库的指定路径</summary>
        public const String RootPath = "RootPath";

        /// <summary>协议。http或https，留空表示用原来协议</summary>
        public const String Protocol = "Protocol";

        /// <summary>同步周期。默认60秒</summary>
        public const String Period = "Period";

        /// <summary>白名单。仅搜索匹配的文件，支持*，多个规则逗号隔开</summary>
        public const String Whites = "Whites";

        /// <summary>黑名单。仅搜索匹配以外的文件，支持*，多个规则逗号隔开</summary>
        public const String Blacks = "Blacks";

        /// <summary>最后扫描。记录最后一次扫描时间</summary>
        public const String LastScan = "LastScan";

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
