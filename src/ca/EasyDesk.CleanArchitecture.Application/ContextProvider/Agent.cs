using EasyDesk.Commons.Collections;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Agent
{
    public Agent(IImmutableDictionary<Realm, Identity> identities)
    {
        if (identities.Count == 0)
        {
            throw new ArgumentException("An agent must have at least one identity.", nameof(identities));
        }
        Identities = identities;
    }

    public IImmutableDictionary<Realm, Identity> Identities { get; }

    public Identity RequireIdentity(Realm realm) => GetIdentity(realm)
        .OrElseThrow(() => new InvalidOperationException($"Missing required identity with name {realm}."));

    public Option<Identity> GetIdentity(Realm realm) => Identities.GetOption(realm);

    public static Agent FromIdentities(IEnumerable<Identity> identities) =>
        new(identities.ToEquatableMap(x => x.Realm, x => x));

    public static Agent FromIdentities(params Identity[] identities) =>
        FromIdentities(identities.AsEnumerable());

    public static Agent FromSingleIdentity(Identity identity) =>
        FromIdentities(identity);

    public static Agent FromSingleIdentity(Realm realm, IdentityId id) =>
        FromSingleIdentity(new Identity(realm, id));

    public static Agent FromSingleIdentity(Realm realm, IdentityId id, AttributeCollection attributes) =>
        FromSingleIdentity(new Identity(realm, id, attributes));

    public static Agent Construct(Action<AgentBuilder> configure)
    {
        var builder = new AgentBuilder();
        configure(builder);
        return builder.Build();
    }
}
