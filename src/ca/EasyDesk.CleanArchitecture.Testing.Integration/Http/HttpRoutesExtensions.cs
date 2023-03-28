namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public static class HttpRoutesExtensions
{
    public static string WithRouteParam(this string route, string paramName, object value)
    {
        return route.Replace($$"""{{{paramName}}}""", value.ToString());
    }
}
