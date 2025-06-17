using EasyDesk.CleanArchitecture.Application.Authentication;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public interface ITestHttpAuthentication
{
    ImmutableHttpRequestMessage ConfigureAuthentication(
        ImmutableHttpRequestMessage message,
        Agent agent);

    ImmutableHttpRequestMessage RemoveAuthentication(
        ImmutableHttpRequestMessage message);
}

internal class NoAuthentication : ITestHttpAuthentication
{
    public ImmutableHttpRequestMessage ConfigureAuthentication(
        ImmutableHttpRequestMessage message,
        Agent agent) =>
        message;

    public ImmutableHttpRequestMessage RemoveAuthentication(
        ImmutableHttpRequestMessage message) =>
        message;
}
