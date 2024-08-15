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

/// <summary>文件仓库。文件存储的根目录</summary>
[Serializable]
[DataObject]
[Description("文件仓库。文件存储的根目录")]
[BindIndex("IU_FileStorage_Name", true, "Name")]
[BindTable("FileStorage", Description = "文件仓库。文件存储的根目录", ConnName = "EasyFile", DbType = DatabaseType.None)]
public partial class FileStorage
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

    private Boolean _Enable;
    /// <summary>启用</summary>
    [DisplayName("启用")]
    [Description("启用")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Enable", "启用", "")]
    public Boolean Enable { get => _Enable; set { if (OnPropertyChanging("Enable", value)) { _Enable = value; OnPropertyChanged("Enable"); } } }

    private String _HomeDirectory;
    /// <summary>主目录</summary>
    [DisplayName("主目录")]
    [Description("主目录")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("HomeDirectory", "主目录", "")]
    public String HomeDirectory { get => _HomeDirectory; set { if (OnPropertyChanging("HomeDirectory", value)) { _HomeDirectory = value; OnPropertyChanged("HomeDirectory"); } } }

    private Int64 _Size;
    /// <summary>大小。总大小</summary>
    [DisplayName("大小")]
    [Description("大小。总大小")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Size", "大小。总大小", "", ItemType = "GMK")]
    public Int64 Size { get => _Size; set { if (OnPropertyChanging("Size", value)) { _Size = value; OnPropertyChanged("Size"); } } }

    private NewLife.Remoting.Models.CommandStatus _Status;
    /// <summary>状态</summary>
    [DisplayName("状态")]
    [Description("状态")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Status", "状态", "")]
    public NewLife.Remoting.Models.CommandStatus Status { get => _Status; set { if (OnPropertyChanging("Status", value)) { _Status = value; OnPropertyChanged("Status"); } } }

    private Int32 _Level;
    /// <summary>深度。最大搜索目录深度</summary>
    [DisplayName("深度")]
    [Description("深度。最大搜索目录深度")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Level", "深度。最大搜索目录深度", "")]
    public Int32 Level { get => _Level; set { if (OnPropertyChanging("Level", value)) { _Level = value; OnPropertyChanged("Level"); } } }

    private Int32 _Period;
    /// <summary>同步周期。默认60秒</summary>
    [DisplayName("同步周期")]
    [Description("同步周期。默认60秒")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Period", "同步周期。默认60秒", "")]
    public Int32 Period { get => _Period; set { if (OnPropertyChanging("Period", value)) { _Period = value; OnPropertyChanged("Period"); } } }

    private String _Pattern;
    /// <summary>匹配规则。仅搜索匹配的文件，支持*，多个规则逗号隔开</summary>
    [DisplayName("匹配规则")]
    [Description("匹配规则。仅搜索匹配的文件，支持*，多个规则逗号隔开")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Pattern", "匹配规则。仅搜索匹配的文件，支持*，多个规则逗号隔开", "")]
    public String Pattern { get => _Pattern; set { if (OnPropertyChanging("Pattern", value)) { _Pattern = value; OnPropertyChanged("Pattern"); } } }

    private EasyWeb.Models.RedirectModes _RedirectMode;
    /// <summary>原始跳转。跳转到原始地址</summary>
    [DisplayName("原始跳转")]
    [Description("原始跳转。跳转到原始地址")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("RedirectMode", "原始跳转。跳转到原始地址", "")]
    public EasyWeb.Models.RedirectModes RedirectMode { get => _RedirectMode; set { if (OnPropertyChanging("RedirectMode", value)) { _RedirectMode = value; OnPropertyChanged("RedirectMode"); } } }

    private String _VipUrl;
    /// <summary>VIP地址。CDN域名地址，加速下载</summary>
    [DisplayName("VIP地址")]
    [Description("VIP地址。CDN域名地址，加速下载")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("VipUrl", "VIP地址。CDN域名地址，加速下载", "")]
    public String VipUrl { get => _VipUrl; set { if (OnPropertyChanging("VipUrl", value)) { _VipUrl = value; OnPropertyChanged("VipUrl"); } } }

    private String _VipKey;
    /// <summary>VIP密钥。CDN的URL验证密钥</summary>
    [DisplayName("VIP密钥")]
    [Description("VIP密钥。CDN的URL验证密钥")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("VipKey", "VIP密钥。CDN的URL验证密钥", "")]
    public String VipKey { get => _VipKey; set { if (OnPropertyChanging("VipKey", value)) { _VipKey = value; OnPropertyChanged("VipKey"); } } }

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
            "Enable" => _Enable,
            "HomeDirectory" => _HomeDirectory,
            "Size" => _Size,
            "Status" => _Status,
            "Level" => _Level,
            "Period" => _Period,
            "Pattern" => _Pattern,
            "RedirectMode" => _RedirectMode,
            "VipUrl" => _VipUrl,
            "VipKey" => _VipKey,
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
                case "Enable": _Enable = value.ToBoolean(); break;
                case "HomeDirectory": _HomeDirectory = Convert.ToString(value); break;
                case "Size": _Size = value.ToLong(); break;
                case "Status": _Status = (NewLife.Remoting.Models.CommandStatus)value.ToInt(); break;
                case "Level": _Level = value.ToInt(); break;
                case "Period": _Period = value.ToInt(); break;
                case "Pattern": _Pattern = Convert.ToString(value); break;
                case "RedirectMode": _RedirectMode = (EasyWeb.Models.RedirectModes)value.ToInt(); break;
                case "VipUrl": _VipUrl = Convert.ToString(value); break;
                case "VipKey": _VipKey = Convert.ToString(value); break;
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
    #endregion

    #region 扩展查询
    #endregion

    #region 字段名
    /// <summary>取得文件仓库字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>名称</summary>
        public static readonly Field Name = FindByName("Name");

        /// <summary>启用</summary>
        public static readonly Field Enable = FindByName("Enable");

        /// <summary>主目录</summary>
        public static readonly Field HomeDirectory = FindByName("HomeDirectory");

        /// <summary>大小。总大小</summary>
        public static readonly Field Size = FindByName("Size");

        /// <summary>状态</summary>
        public static readonly Field Status = FindByName("Status");

        /// <summary>深度。最大搜索目录深度</summary>
        public static readonly Field Level = FindByName("Level");

        /// <summary>同步周期。默认60秒</summary>
        public static readonly Field Period = FindByName("Period");

        /// <summary>匹配规则。仅搜索匹配的文件，支持*，多个规则逗号隔开</summary>
        public static readonly Field Pattern = FindByName("Pattern");

        /// <summary>原始跳转。跳转到原始地址</summary>
        public static readonly Field RedirectMode = FindByName("RedirectMode");

        /// <summary>VIP地址。CDN域名地址，加速下载</summary>
        public static readonly Field VipUrl = FindByName("VipUrl");

        /// <summary>VIP密钥。CDN的URL验证密钥</summary>
        public static readonly Field VipKey = FindByName("VipKey");

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

    /// <summary>取得文件仓库字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>名称</summary>
        public const String Name = "Name";

        /// <summary>启用</summary>
        public const String Enable = "Enable";

        /// <summary>主目录</summary>
        public const String HomeDirectory = "HomeDirectory";

        /// <summary>大小。总大小</summary>
        public const String Size = "Size";

        /// <summary>状态</summary>
        public const String Status = "Status";

        /// <summary>深度。最大搜索目录深度</summary>
        public const String Level = "Level";

        /// <summary>同步周期。默认60秒</summary>
        public const String Period = "Period";

        /// <summary>匹配规则。仅搜索匹配的文件，支持*，多个规则逗号隔开</summary>
        public const String Pattern = "Pattern";

        /// <summary>原始跳转。跳转到原始地址</summary>
        public const String RedirectMode = "RedirectMode";

        /// <summary>VIP地址。CDN域名地址，加速下载</summary>
        public const String VipUrl = "VipUrl";

        /// <summary>VIP密钥。CDN的URL验证密钥</summary>
        public const String VipKey = "VipKey";

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
