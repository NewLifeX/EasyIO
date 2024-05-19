using System.ComponentModel;
using NewLife;
using NewLife.Cube;

namespace EasyWeb.Areas.Files;

[DisplayName("文件管理")]
public class FilesArea : AreaBase
{
    public FilesArea() : base(nameof(FilesArea).TrimEnd("Area")) { }
}