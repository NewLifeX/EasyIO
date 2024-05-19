using System.ComponentModel;
using NewLife.Cube;

namespace EasyWeb.Areas.Files.Controllers;

[DisplayName("文件配置")]
[Menu(1, true, Icon = "fa-table")]
[FilesArea]
public class EasyFileController : ConfigController<EasyFileSetting>
{
}
