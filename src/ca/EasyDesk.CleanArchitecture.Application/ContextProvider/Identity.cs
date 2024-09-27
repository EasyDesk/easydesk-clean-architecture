namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Identity(Realm Realm, IdentityId Id, AttributeCollection Attributes)
{
    public Identity(Realm realm, IdentityId id) : this(realm, id, AttributeCollection.Empty)
    {
    }

    public bool HasId(IdentityId id) => Id == id;

    public static Identity Create(Realm realm, IdentityId id, params (string Key, string Value)[] attributes) =>
        Create(realm, id, attributes.AsEnumerable());

    public static Identity Create(Realm realm, IdentityId id, IEnumerable<(string Key, string Value)> attributes) =>
        new(realm, id, AttributeCollection.FromFlatKeyValuePairs(attributes));
}
