using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public interface ITestHttpAuthentication
{
    ImmutableHttpRequestMessage ConfigureAuthentication(
        ImmutableHttpRequestMessage message,
        IEnumerable<Claim> identity);

    ImmutableHttpRequestMessage RemoveAuthentication(
        ImmutableHttpRequestMessage message);
}

internal class NoAuthentication : ITestHttpAuthentication
{
    public ImmutableHttpRequestMessage ConfigureAuthentication(
        ImmutableHttpRequestMessage message,
        IEnumerable<Claim> identity) =>
        message;

    public ImmutableHttpRequestMessage RemoveAuthentication(
        ImmutableHttpRequestMessage message) =>
        message;
}
