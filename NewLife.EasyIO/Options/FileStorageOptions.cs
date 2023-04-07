using NewLife;
using NewLife.Configuration;
using NewLife.Log;
using NewLife.Model;

namespace NewLife.EasyIO.Options;

/// <summary>文件存储选型</summary>
public class FileStorageOptions : IConfigMapping
{
    public String Path { get; set; }

    public FileStorageOptions() { }

    public FileStorageOptions(IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetService<IConfigProvider>();
        config.Bind(this, true, "easyio");

        XTrace.WriteLine("文件存储：{0}", Path);
    }

    void IConfigMapping.MapConfig(IConfigProvider provider, IConfigSection section) => section?.MapTo(this, provider);
}