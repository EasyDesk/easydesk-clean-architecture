using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Shouldly;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Context;

public class AgentParserBuilderTests
{
    private readonly Realm _realmA = Realm.New("realmA");
    private readonly Realm _realmB = Realm.New("realmB");

    [Fact]
    public void ShouldParseSingleIdentities()
    {
        var identityId = IdentityId.New("some-id");
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
            identityId,
            ("x", "x-value"),
            ("y", "y-value"));

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(_realmA, identity));
    }

    [Fact]
    public void ShouldParseMultipleIdentities()
    {
        var identityIdA = IdentityId.New("id-A");
        var identityIdB = IdentityId.New("id-B");
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
            identityIdA,
            ("x", "x-value"),
            ("y", "y-value"));
        var identityB = Identity.Create(identityIdB);

        parser(claimsPrincipal).ShouldContain(Agent.FromIdentities(
            (_realmA, identityA),
            (_realmB, identityB)));
    }

    [Fact]
    public void ShouldSucceedWithMissingOptionalIdentity()
    {
        var identityIdA = IdentityId.New("id-A");
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
            identityIdA,
            ("x", "x-value"),
            ("y", "y-value"));

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(_realmA, identityA));
    }

    [Fact]
    public void ShouldIgnoreMissingAttributes()
    {
        var identityId = IdentityId.New("some-id");
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
            identityId,
            ("x", "x-value"));

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(_realmA, identity));
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
