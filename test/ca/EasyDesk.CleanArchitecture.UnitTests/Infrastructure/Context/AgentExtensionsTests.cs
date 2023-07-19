using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Testing;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Context;

public class AgentExtensionsTests
{
    private readonly FakeClock _clock = new(SystemClock.Instance.GetCurrentInstant());
    private readonly JwtFacade _jwtFacade;
    private readonly SecurityKey _key = KeyUtils.KeyFromString(new string('X', 64));

    public AgentExtensionsTests()
    {
        _jwtFacade = new JwtFacade(_clock);
    }

    [Theory]
    [MemberData(nameof(Agents))]
    public void ForwardAndBackwardConversionShouldBeIdempotent(Agent agent)
    {
        var claimsIdentity = agent.ToClaimsIdentity();
        var jwt = _jwtFacade.Create(claimsIdentity, x => x
            .WithSigningCredentials(new SigningCredentials(_key, SecurityAlgorithms.HmacSha256))
            .WithLifetime(Duration.FromDays(1)));
        var result = _jwtFacade.Validate(jwt, x => x.WithSignatureValidation(_key));
        result.Map(x => x.ToAgent()).ShouldBe(agent);
    }

    public static IEnumerable<object[]> Agents()
    {
        var realmA = Realm.New("realmA");
        var realmB = Realm.New("realmB");
        var idA = IdentityId.New("AAAA");
        var idB = IdentityId.New("BBBB");
        yield return new[]
        {
            Agent.Construct(x => x.AddIdentity(realmA, idA))
        };
        yield return new[]
        {
            Agent.Construct(x =>
            {
                x.AddIdentity(realmA, idA);
                x.AddIdentity(realmB, idB);
            })
        };
        yield return new[]
        {
            Agent.Construct(x =>
            {
                x.AddIdentity(realmA, idA)
                    .AddAttribute("attributeA", "1")
                    .AddAttribute("attributeB", "2");
                x.AddIdentity(realmB, idB);
            })
        };
        yield return new[]
        {
            Agent.Construct(x =>
            {
                x.AddIdentity(realmA, idA)
                    .AddAttribute("attributeA", "1")
                    .AddAttribute("attributeA", "2")
                    .AddAttribute("attributeB", "3");
                x.AddIdentity(realmB, idB);
            })
        };
    }
}
