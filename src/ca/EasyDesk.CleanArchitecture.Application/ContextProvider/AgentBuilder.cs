using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public class AgentBuilder
{
    private readonly Dictionary<Realm, IdentityBuilder> _identities = new();

    public IdentityBuilder AddIdentity(Realm realm, IdentityId id)
    {
        var identityBuilder = new IdentityBuilder(id);
        _identities.Add(realm, identityBuilder);
        return identityBuilder;
    }

    public Agent Build()
    {
        if (_identities.IsEmpty())
        {
            throw new InvalidOperationException("An agent must have at least one identity.");
        }
        return Agent.FromIdentities(_identities.Select(x => (x.Key, x.Value.Build())));
    }
}
