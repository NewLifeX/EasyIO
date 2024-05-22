using EasyWeb.Data;
using EasyWeb.Services;
using Xunit;

namespace XUnitTest;

public class EntryServiceTests
{
    [Fact]
    public void FixVersionAndTag()
    {
        var name = "aspnetcore-runtime-6.0.3-linux-x64.tar.gz";
        var entry = new FileEntry { Name = name };

        var service = new EntryService(null, null);
        service.FixVersionAndTag(entry);

        Assert.Equal(name, entry.Name);
        Assert.Equal(6000300, entry.Version);
        Assert.Equal("aspnetcore-runtime-linux-x64", entry.Tag);
    }
}
