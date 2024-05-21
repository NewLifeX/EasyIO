namespace EasyWeb.Models;

public class ReleaseVersion
{
    public String Version { get; set; }

    public IList<ReleaseFile> Files { get; set; }
}
