using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Shouldly;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Context;

public class AgentParserBuilderTests
{
    [Fact]
    public void ShouldParseSingleIdentities()
    {
        var identityId = IdentityId.New("some-id");
        var parser = ClaimsPrincipalParsers.ForAgent(x => x
            .WithIdentity(Agent.DefaultIdentityName, ClaimTypes.NameIdentifier)
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

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(identity));
    }

    [Fact]
    public void ShouldParseMultipleIdentities()
    {
        var identityIdA = IdentityId.New("id-A");
        var identityIdB = IdentityId.New("id-B");
        var parser = ClaimsPrincipalParsers.ForAgent(x =>
        {
            x.WithIdentity("identityA", "idA")
                .WithAttribute("x", "x")
                .WithAttribute("y", "y");
            x.WithIdentity("identityB", "idB");
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
            ("identityA", identityA),
            ("identityB", identityB)));
    }

    [Fact]
    public void ShouldSucceedWithMissingOptionalIdentity()
    {
        var identityIdA = IdentityId.New("id-A");
        var parser = ClaimsPrincipalParsers.ForAgent(x =>
        {
            x.WithIdentity("identityA", "idA")
                .WithAttribute("x", "x")
                .WithAttribute("y", "y");
            x.WithIdentity("identityB", "idB", required: false);
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

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(
            identityA, name: "identityA"));
    }

    [Fact]
    public void ShouldIgnoreMissingAttributes()
    {
        var identityId = IdentityId.New("some-id");
        var parser = ClaimsPrincipalParsers.ForAgent(x => x
            .WithIdentity(Agent.DefaultIdentityName, ClaimTypes.NameIdentifier)
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

        parser(claimsPrincipal).ShouldContain(Agent.FromSingleIdentity(identity));
    }

    [Fact]
    public void ShouldFailIfNoIdentityIsDetected()
    {
        var parser = ClaimsPrincipalParsers.ForAgent(x =>
        {
            x.WithIdentity("identityA", "idA", required: false)
                .WithAttribute("x", "x")
                .WithAttribute("y", "y");
            x.WithIdentity("identityB", "idB", required: false);
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
