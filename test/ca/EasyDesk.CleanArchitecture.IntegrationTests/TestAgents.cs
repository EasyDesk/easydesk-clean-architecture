using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.SampleApp.Application.Authorization;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public static class TestAgents
{
    public static Agent Admin { get; } = Agent.Construct(agent =>
    {
        agent
            .AddIdentity(Realms.MainRealm, new IdentityId("test-admin"))
            .AddAttribute(StandardAttributes.FirstName, "John")
            .AddAttribute(StandardAttributes.LastName, "Doe")
            .AddAttribute(StandardAttributes.Email, "johndoe@test.com");
    });

    public static Agent OtherUser { get; } = Agent.Construct(agent =>
    {
        agent
            .AddIdentity(Realms.MainRealm, new IdentityId("test-user"))
            .AddAttribute(StandardAttributes.FirstName, "Carl")
            .AddAttribute(StandardAttributes.LastName, "Smith")
            .AddAttribute(StandardAttributes.Email, "carlsmith@test.com");
    });
}
