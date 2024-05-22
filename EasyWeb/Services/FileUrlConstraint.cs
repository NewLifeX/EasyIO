namespace EasyWeb.Services;

/// <summary>信息路径适配</summary>
class FileUrlConstraint : IRouteConstraint
{
    private readonly EntryService _entryService;

    public FileUrlConstraint(EntryService entryService) => _entryService = entryService;

    public Boolean Match(HttpContext httpContext, IRouter route, String parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    {
        var file = _entryService.GetEntry(0, values["file"] + "");
        if (file == null) return false;

        return true;
    }
}
