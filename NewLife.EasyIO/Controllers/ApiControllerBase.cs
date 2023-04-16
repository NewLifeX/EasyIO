using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Serialization;
using IActionFilter = Microsoft.AspNetCore.Mvc.Filters.IActionFilter;

namespace NewLife.EasyWeb.Controllers;

/// <summary>控制器基类</summary>
[ApiController]
[Produces("application/json")]
[Route("[controller]/[action]")]
public class ApiControllerBase : ControllerBase, IActionFilter
{
    #region 属性
    /// <summary>令牌</summary>
    public String Token { get; set; }
    #endregion

    #region 构造
    /// <summary>动作执行前</summary>
    /// <param name="context"></param>
    [NonAction]
    public virtual void OnActionExecuting(ActionExecutingContext context)
    {
        // 访问令牌
        var request = context.HttpContext.Request;
        var token = request.Query["Token"] + "";
        if (token.IsNullOrEmpty()) token = (request.Headers["Authorization"] + "").TrimStart("Bearer ");
        if (token.IsNullOrEmpty()) token = request.Headers["X-Token"] + "";
        if (token.IsNullOrEmpty()) token = request.Cookies["Token"] + "";
        Token = token;

        try
        {
            //todo 令牌验证
            //if (token.IsNullOrEmpty() && context.ActionDescriptor is ControllerActionDescriptor act && !act.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute)))
            //    throw new ApiException(403, "认证失败");
        }
        catch (Exception ex)
        {
            context.Result = Json(0, null, ex);
        }
    }

    /// <summary>动作执行后</summary>
    /// <param name="context"></param>
    [NonAction]
    public virtual void OnActionExecuted(ActionExecutedContext context)
    {
        var ex = context.Exception?.GetTrue();
        var traceId = DefaultSpan.Current?.TraceId;

        if (context.Result != null)
        {
            if (context.Result is ObjectResult obj)
            {
                var rs = new { code = obj.StatusCode ?? 0, data = obj.Value, traceId };
                context.Result = new ContentResult
                {
                    Content = OnJsonSerialize(rs),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new JsonResult(new { code = 0, data = new { }, traceId });
            }
        }
        else if (context.Exception != null && !context.ExceptionHandled)
        {
            if (ex is ApiException aex)
                context.Result = new JsonResult(new { code = aex.Code, data = aex.Message, traceId });
            else
                context.Result = new JsonResult(new { code = 500, data = ex.Message, traceId });

            context.ExceptionHandled = true;

            // 输出异常日志
            if (XTrace.Debug) XTrace.WriteException(ex);
        }
    }
    #endregion

    #region Json结果
    /// <summary>响应Json结果</summary>
    /// <param name="code">代码。0成功，其它为错误代码</param>
    /// <param name="message">消息，成功或失败时的文本消息</param>
    /// <param name="data">数据对象</param>
    /// <param name="extend">扩展数据</param>
    /// <returns></returns>
    [NonAction]
    public virtual ActionResult Json(Int32 code, String message, Object data = null, Object extend = null)
    {
        if (data is Exception ex)
        {
            if (code == 0 && data is ApiException aex) code = aex.Code;
            if (code == 0) code = 500;
            if (message.IsNullOrEmpty()) message = ex.GetTrue()?.Message;
            data = null;
        }

        Object rs = new { code, message, data };
        if (extend != null)
        {
            var dic = rs.ToDictionary();
            dic.Merge(extend);
            rs = dic;
        }

        var json = OnJsonSerialize(rs);
        DefaultSpan.Current?.AppendTag(json);

        return Content(json, "application/json", Encoding.UTF8);
    }

    /// <summary>Json序列化。默认使用FastJson</summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected virtual String OnJsonSerialize(Object data)
    {
        //data.ToJson(false, true, true);
        var writer = new JsonWriter
        {
            Indented = false,
            IgnoreNullValues = false,
            CamelCase = true,
            Int64AsString = true
        };

        writer.Write(data);

        return writer.GetString();
    }
    #endregion
}