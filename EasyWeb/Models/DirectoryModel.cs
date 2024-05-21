using EasyWeb.Data;

namespace EasyWeb.Models;

public class DirectoryModel
{
    public FileEntry Parent { get; set; }

    public IList<FileEntry> Entries { get; set; }
}
