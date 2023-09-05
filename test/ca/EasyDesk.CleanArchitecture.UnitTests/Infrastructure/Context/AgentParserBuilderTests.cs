using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Shouldly;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Context;

public class AgentParserBuilderTests
{
    private readonly Realm _realmA = new("realmA");
    private readonly Realm _realmB = new("realmB");

    [Fact]
    public void ShouldParseSingleIdentities()
    {
        var identityId = new IdentityId("some-id");
        var parser = ClaimsPrincipalParsers.ForAgent(x => x
            .WithIdentity(_realmA, ClaimTypes.NameIdentifier)
                .WithAttribute("x", "x")
                .WithAttribute("y", "y"));

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identityId),
            new Claim("x", "x-value"),
            new Claim("y", "y-value"),
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var identity = Identity.Create(
            _realmA,
            identityId,
            ("x", "x-value"),
            ("y", "y-value"));

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(identity));
    }

    [Fact]
    public void ShouldParseMultipleIdentities()
    {
        var identityIdA = new IdentityId("id-A");
        var identityIdB = new IdentityId("id-B");
        var parser = ClaimsPrincipalParsers.ForAgent(x =>
        {
            x.WithIdentity(_realmA, "idA")
                .WithAttribute("x", "x")
                .WithAttribute("y", "y");
            x.WithIdentity(_realmB, "idB");
        });

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("idA", identityIdA),
            new Claim("idB", identityIdB),
            new Claim("x", "x-value"),
            new Claim("y", "y-value"),
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var identityA = Identity.Create(
            _realmA,
            identityIdA,
            ("x", "x-value"),
            ("y", "y-value"));
        var identityB = Identity.Create(
            _realmB,
            identityIdB);

        parser(claimsPrincipal).ShouldContain(Agent.FromIdentities(identityA, identityB));
    }

    [Fact]
    public void ShouldSucceedWithMissingOptionalIdentity()
    {
        var identityIdA = new IdentityId("id-A");
        var parser = ClaimsPrincipalParsers.ForAgent(x =>
        {
            x.WithIdentity(_realmA, "idA")
                .WithAttribute("x", "x")
                .WithAttribute("y", "y");
            x.WithIdentity(_realmB, "idB", required: false);
        });

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("idA", identityIdA),
            new Claim("x", "x-value"),
            new Claim("y", "y-value"),
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var identityA = Identity.Create(
            _realmA,
            identityIdA,
            ("x", "x-value"),
            ("y", "y-value"));

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(identityA));
    }

    [Fact]
    public void ShouldIgnoreMissingAttributes()
    {
        var identityId = new IdentityId("some-id");
        var parser = ClaimsPrincipalParsers.ForAgent(x => x
            .WithIdentity(_realmA, ClaimTypes.NameIdentifier)
                .WithAttribute("x", "x")
                .WithAttribute("y", "y"));

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identityId),
            new Claim("x", "x-value"),
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var identity = Identity.Create(
            _realmA,
            identityId,
            ("x", "x-value"));

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(identity));
    }

    [Fact]
    public void ShouldFailIfNoIdentityIsDetected()
    {
        var parser = ClaimsPrincipalParsers.ForAgent(x =>
        {
            x.WithIdentity(_realmA, "idA", required: false)
                .WithAttribute("x", "x")
                .WithAttribute("y", "y");
            x.WithIdentity(_realmB, "idB", required: false);
        });

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("x", "x-value"),
            new Claim("y", "y-value"),
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        parser(claimsPrincipal).ShouldBeEmpty();
    }
}
