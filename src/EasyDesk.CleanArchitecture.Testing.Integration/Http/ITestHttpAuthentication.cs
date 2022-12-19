using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public interface ITestHttpAuthentication
{
    void ConfigureAuthentication(HttpRequestBuilder builder, IEnumerable<Claim> identity);

    void RemoveAuthentication(HttpRequestBuilder builder);

    public static ITestHttpAuthentication NoAuthentication => new NoAuthentication();
}

internal class NoAuthentication : ITestHttpAuthentication
{
    public void ConfigureAuthentication(HttpRequestBuilder builder, IEnumerable<Claim> identity)
    {
    }

    public void RemoveAuthentication(HttpRequestBuilder builder)
    {
    }
}
