using System.ComponentModel;

namespace EasyWeb.Models;

public enum RedirectModes
{
    /// <summary>默认</summary>
    [Description("默认")]
    None = 0,

    /// <summary>原始跳转</summary>
    [Description("原始跳转")]
    Redirect = 1,

    /// <summary>智能鉴权。没有鉴权的用户才跳转</summary>
    [Description("智能鉴权")]
    Smart = 2,
}
