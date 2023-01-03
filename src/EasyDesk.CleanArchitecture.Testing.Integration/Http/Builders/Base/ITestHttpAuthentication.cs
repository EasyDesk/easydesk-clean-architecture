using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public interface ITestHttpAuthentication
{
    void ConfigureAuthentication(HttpRequestMessage message, IEnumerable<Claim> identity);

    void RemoveAuthentication(HttpRequestMessage message);
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
