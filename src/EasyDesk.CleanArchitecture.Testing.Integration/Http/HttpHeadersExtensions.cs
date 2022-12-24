using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public static class HttpHeadersExtensions
{
    public static void Replace(this HttpRequestHeaders headers, string name, string value)
    {
        headers.Remove(name);
        headers.Add(name, value);
    }

    public static void Replace(this HttpRequestHeaders headers, string name, IEnumerable<string> value)
    {
        headers.Remove(name);
        headers.Add(name, value);
    }
}
