namespace EasyWeb.Models;

public class FileModel
{
    public String Name { get; set; }

    public String Path { get; set; }

    public String ParentPath { get; set; }

    public String Title { get; set; }

    public Int64 Size { get; set; }

    public DateTime LastWrite { get; set; }

    public Boolean IsDirectory { get; set; }

    public String Hash { get; set; }

    public String Url { get; set; }
}
