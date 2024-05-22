using NewLife;
using System.Web;

namespace EasyWeb.Services;

/// <summary>目录路径适配</summary>
class DirUrlConstraint : IRouteConstraint
{
    private readonly EntryService _entryService;

    public DirUrlConstraint(EntryService entryService) => _entryService = entryService;

    public Boolean Match(HttpContext httpContext, IRouter route, String parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    {
        var pathInfo = values["pathInfo"] + "";
        if (pathInfo.IsNullOrEmpty() || pathInfo.StartsWithIgnoreCase("Admin/", "Cube/")) return false;

        pathInfo = HttpUtility.UrlDecode(pathInfo);

        var entry = _entryService.GetEntry(0, pathInfo);
        if (entry == null) return false;

        return entry.Enable && entry.IsDirectory;
    }
}
