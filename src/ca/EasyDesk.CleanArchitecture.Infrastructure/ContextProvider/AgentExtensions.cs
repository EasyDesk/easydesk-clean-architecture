using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using AgentClaimDto = System.Collections.Generic.Dictionary<string, EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.AgentExtensions.IdentityClaimDto>;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public static class AgentExtensions
{
    private const string AgentClaimType = "agent";
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public record IdentityClaimDto(string Id, Dictionary<string, IEnumerable<string>> Attributes);

    public static ClaimsIdentity ToClaimsIdentity(this Agent agent)
    {
        var model = ConvertAgentToClaim(agent);
        var agentClaim = new Claim(
            AgentClaimType,
            JsonSerializer.Serialize(model, _jsonOptions),
            JsonClaimValueTypes.Json);
        return new ClaimsIdentity(new[] { agentClaim }, "Agent");
    }

    private static AgentClaimDto ConvertAgentToClaim(Agent agent) =>
        agent.Identities.ToDictionary(x => x.Key.Value, x => ConvertIdentityToClaim(x.Value));

    private static IdentityClaimDto ConvertIdentityToClaim(Identity identity) =>
        new(identity.Id, identity.Attributes.AttributeMap.ToDictionary(x => x.Key, x => x.Value.AsEnumerable()));

    public static Agent ToAgent(this ClaimsIdentity claimsIdentity)
    {
        var agentClaim = claimsIdentity.RequireClaim(AgentClaimType);
        var agentModel = JsonSerializer.Deserialize<AgentClaimDto>(agentClaim, _jsonOptions)
            ?? throw new InvalidOperationException("Json claim deserialization returned null.");
        return ConvertClaimToAgent(agentModel);
    }

    public static Agent ConvertClaimToAgent(AgentClaimDto claim) =>
        Agent.FromIdentities(claim.Select(x => ConvertClaimToIdentity(x.Value, x.Key)));

    public static Identity ConvertClaimToIdentity(IdentityClaimDto claim, string realm) =>
        new(new Realm(realm), new IdentityId(claim.Id), new(claim.Attributes.ToFixedMap(x => x.Key, x => x.Value.ToFixedSet())));

    private static string RequireClaim(this ClaimsIdentity claimsIdentity, string claimType)
    {
        return claimsIdentity
            .FindFirst(claimType)
            .AsOption()
            .Map(c => c.Value)
            .OrElseThrow(() => new InvalidOperationException($"Claims identity contains no claim with type '{claimType}'."));
    }
}
