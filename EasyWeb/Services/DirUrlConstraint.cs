namespace EasyWeb.Services;

/// <summary>目录路径适配</summary>
class DirUrlConstraint : IRouteConstraint
{
    private readonly EntryService _entryService;

    public DirUrlConstraint(EntryService entryService) => _entryService = entryService;

    public Boolean Match(HttpContext httpContext, IRouter route, String parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    {
        var entry = _entryService.GetFile(0, values["pathInfo"] + "");
        if (entry == null) return false;

        return entry.IsDirectory;
    }
}
