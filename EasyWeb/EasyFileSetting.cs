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
    [Description("根目录。文件管理根目录")]
    public String Root { get; set; } = "../files";
    #endregion
}
