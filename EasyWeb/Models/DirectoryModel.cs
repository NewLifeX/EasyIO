namespace EasyWeb.Models;

public class DirectoryModel
{
    public IList<FileModel> Parents { get; set; }

    public IList<FileModel> Entries { get; set; }
}
