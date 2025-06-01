using System.ComponentModel;
using NewLife.Configuration;
using XCode.Configuration;

namespace EasyWeb;

[Config("EasyFile")]
public class EasyFileSetting : Config<EasyFileSetting>
{
    #region 静态
    static EasyFileSetting() => Provider = new DbConfigProvider { Category = "EasyFile" };
    #endregion

    #region 属性
    /// <summary>根目录。文件管理根目录</summary>
    [Description("根目录。文件管理根目录")]
    public String Root { get; set; } = "../files";

    /// <summary>限流周期。在指定限流周期内，流量超限时返回TooManyRequests，默认600秒</summary>
    [Description("限流周期。在指定限流周期内，流量超限时返回TooManyRequests，默认600秒")]
    public Int32 LimitCycle { get; set; } = 600;

    /// <summary>IP限流流量。在指定限流周期内，流量超限时返回TooManyRequests，默认200M</summary>
    [Description("IP限流流量。在指定限流周期内，流量超限时返回TooManyRequests，默认200M")]
    public Int32 FlowLimitByIP { get; set; } = 200;
    #endregion
}
