using NewLife.Data;
using NewLife.Http;
using NewLife.Log;
using NewLife.Remoting;

namespace NewLife.EasyIO;

/// <summary>文件存储客户端</summary>
/// <remarks>
/// 使用方式，可以引用sdk，也可以直接把 EasyClient 类抠出来使用。
/// </remarks>
public class EasyClient
{
    #region 属性
    /// <summary>服务端地址</summary>
    public String Server { get; set; }

    /// <summary>应用标识</summary>
    public String AppId { get; set; }

    /// <summary>应用密钥</summary>
    public String Secret { get; set; }

    private ApiHttpClient _client;
    #endregion

    #region 基础方法
    private ApiHttpClient GetClient()
    {
        if (_client == null)
        {
            if (Server.IsNullOrEmpty()) throw new ArgumentNullException(nameof(Server));
            //if (AppId.IsNullOrEmpty()) throw new ArgumentNullException(nameof(AppId));

            // 支持多服务器地址，支持负载均衡
            var client = new ApiHttpClient(Server);

            if (!AppId.IsNullOrEmpty())
                client.Filter = new TokenHttpFilter { UserName = AppId, Password = Secret };

            _client = client;
        }

        return _client;
    }
    #endregion

    #region 文件管理
    /// <summary>上传对象</summary>
    /// <param name="id">对象标识。支持斜杠目录结构</param>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual async Task<String> Put(String id, Packet data)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var client = GetClient();
        return await client.PostAsync<String>($"/io/put?id={id}", data);
    }

    /// <summary>根据Id获取对象</summary>
    /// <param name="id">对象标识。支持斜杠目录结构</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual async Task<Packet> Get(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var client = GetClient();
        return await client.GetAsync<Packet>($"/io/get", new { id });
    }

    /// <summary>获取对象下载Url</summary>
    /// <param name="id">对象标识。支持斜杠目录结构</param>
    /// <returns></returns>
    public virtual async Task<String> GetUrl(String id)
    {
        if (id.IsNullOrEmpty()) throw new ArgumentNullException(nameof(id));

        var client = GetClient();
        return await client.GetAsync<String>($"/io/geturl", new { id });
    }
    #endregion

    #region 辅助
    /// <summary>性能追踪</summary>
    public ITracer Tracer { get; set; }

    /// <summary>日志</summary>
    public ILog Log { get; set; }

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
    #endregion
}