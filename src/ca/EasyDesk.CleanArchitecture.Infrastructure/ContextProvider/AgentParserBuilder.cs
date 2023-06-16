using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public class AgentParserBuilder
{
    private readonly IDictionary<string, IdentityParserBuilder> _identities =
        new Dictionary<string, IdentityParserBuilder>();

    private readonly ISet<string> _requiredIdentities = new HashSet<string>();

    public IdentityParserBuilder WithIdentity(
        string name,
        ClaimsPrincipalParser<IdentityId> id,
        bool required = true)
    {
        var builder = new IdentityParserBuilder(id);
        _identities.Add(name, builder);
        if (required)
        {
            _requiredIdentities.Add(name);
        }
        return builder;
    }

    public IdentityParserBuilder WithIdentity(
        string name,
        string idClaim,
        bool required = true)
    {
        return WithIdentity(
            name,
            ClaimsPrincipalParsers.ForClaim(idClaim).Map(IdentityId.New),
            required);
    }

    public ClaimsPrincipalParser<Agent> Build()
    {
        var identityParsers = _identities.Select(x => (x.Key, x.Value.Build())).ToList();
        return claimsPrincipal =>
        {
            var identities = new List<(string, Identity)>();
            foreach (var (name, parser) in identityParsers)
            {
                var parsedIdentity = parser(claimsPrincipal);
                if (IsRequired(name) && parsedIdentity.IsAbsent)
                {
                    return None;
                }
                parsedIdentity.IfPresent(i => identities.Add((name, i)));
            }
            return Some(identities)
                .Filter(i => i.Any())
                .Map(Agent.FromIdentities);
        };
    }

    private bool IsRequired(string name) => _requiredIdentities.Contains(name);
}
