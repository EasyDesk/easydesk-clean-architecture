namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Identity(IdentityId Id, AttributeCollection Attributes)
{
    public Identity(IdentityId id) : this(id, AttributeCollection.Empty)
    {
    }

    public bool HasId(IdentityId id) => Id == id;

    public static Identity Create(IdentityId id, params (string Key, string Value)[] attributes) =>
        new(id, AttributeCollection.FromFlatKeyValuePairs(attributes));
}
