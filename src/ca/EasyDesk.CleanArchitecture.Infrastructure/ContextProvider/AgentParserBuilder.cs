using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public class AgentParserBuilder
{
    private readonly IDictionary<Realm, IdentityParserBuilder> _identities =
        new Dictionary<Realm, IdentityParserBuilder>();

    private readonly ISet<Realm> _requiredIdentities = new HashSet<Realm>();

    public IdentityParserBuilder WithIdentity(
        Realm realm,
        ClaimsPrincipalParser<IdentityId> id,
        bool required = true)
    {
        var builder = new IdentityParserBuilder(realm, id);
        _identities.Add(realm, builder);
        if (required)
        {
            _requiredIdentities.Add(realm);
        }
        return builder;
    }

    public IdentityParserBuilder WithIdentity(
        Realm realm,
        string idClaim,
        bool required = true)
    {
        return WithIdentity(
            realm,
            ClaimsPrincipalParsers.ForClaim(idClaim).Map(id => new IdentityId(id)),
            required);
    }

    public ClaimsPrincipalParser<Agent> Build()
    {
        var identityParsers = _identities.Select(x => (x.Key, x.Value.Build())).ToList();
        return claimsPrincipal =>
        {
            var identities = new List<Identity>();
            foreach (var (name, parser) in identityParsers)
            {
                var parsedIdentity = parser(claimsPrincipal);
                if (IsRequired(name) && parsedIdentity.IsAbsent)
                {
                    return None;
                }
                parsedIdentity.IfPresent(i => identities.Add(i));
            }
            return Some(identities)
                .Filter(i => i.HasAny())
                .Map(Agent.FromIdentities);
        };
    }

    private bool IsRequired(Realm realm) => _requiredIdentities.Contains(realm);
}
