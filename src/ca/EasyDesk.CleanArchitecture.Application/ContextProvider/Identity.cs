namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Identity(IdentityId Id, AttributeCollection Attributes)
{
    public Identity(IdentityId id) : this(id, AttributeCollection.Empty)
    {
    }

    public static Identity Create(IdentityId id, params (string Key, string Value)[] attributes) =>
        new(id, AttributeCollection.FromFlatKeyValuePairs(attributes));
}
