using NewLife.Configuration;
using NewLife.Log;

namespace NewLife.EasyIO.Options;

/// <summary>文件存储选型</summary>
public class FileStorageOptions : IConfigMapping
{
    /// <summary>路径</summary>
    public String Path { get; set; }

    /// <summary>实例化</summary>
    public FileStorageOptions() { }

    /// <summary>实例化</summary>
    public FileStorageOptions(IServiceProvider serviceProvider)
    {
        var config = serviceProvider.GetService<IConfigProvider>();
        config.Bind(this, true, "easyio");

        XTrace.WriteLine("文件存储：{0}", Path);
    }

    void IConfigMapping.MapConfig(IConfigProvider provider, IConfigSection section) => section?.MapTo(this, provider);
}