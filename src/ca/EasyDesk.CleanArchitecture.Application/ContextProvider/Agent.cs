using EasyDesk.Commons.Collections;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Agent
{
    public const string DefaultIdentityName = "main";

    public Agent(IImmutableDictionary<string, Identity> identities)
    {
        if (identities.Count == 0)
        {
            throw new ArgumentException("An agent must have at least one identity.", nameof(identities));
        }
        Identities = identities;
    }

    public IImmutableDictionary<string, Identity> Identities { get; }

    public Identity RequireIdentity(string name) => GetIdentity(name).OrElseThrow(() => new InvalidOperationException($"Missing required identity with name {name}."));

    private Option<Identity> GetIdentity(string name) => Identities.GetOption(name);

    public static Agent FromIdentities(IEnumerable<(string, Identity)> identities) =>
        new(identities.ToEquatableMap());

    public static Agent FromIdentities(params (string, Identity)[] identities) =>
        FromIdentities(identities.AsEnumerable());

    public static Agent FromSingleIdentity(Identity identity, string name = DefaultIdentityName) =>
        FromIdentities(new[] { (name, identity) });

    public static Agent FromSingleIdentity(IdentityId id, string name = DefaultIdentityName) =>
        FromSingleIdentity(new Identity(id), name);

    public static Agent FromSingleIdentity(IdentityId id, AttributeCollection attributes, string name = DefaultIdentityName) =>
        FromSingleIdentity(new Identity(id, attributes), name);
}
