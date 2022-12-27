using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public interface ITestHttpAuthentication
{
    void ConfigureAuthentication(HttpRequestMessage message, IEnumerable<Claim> identity);

    void RemoveAuthentication(HttpRequestMessage message);

    public static ITestHttpAuthentication NoAuthentication => new NoAuthentication();
}

internal class NoAuthentication : ITestHttpAuthentication
{
    public void ConfigureAuthentication(HttpRequestMessage message, IEnumerable<Claim> identity)
    {
    }

    public void RemoveAuthentication(HttpRequestMessage message)
    {
    }
}
