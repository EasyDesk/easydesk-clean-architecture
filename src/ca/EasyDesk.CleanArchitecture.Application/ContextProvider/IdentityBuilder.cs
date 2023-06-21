namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public class IdentityBuilder
{
    private readonly IdentityId _id;
    private readonly ISet<(string, string)> _attributes = new HashSet<(string, string)>();

    public IdentityBuilder(IdentityId id)
    {
        _id = id;
    }

    public IdentityBuilder AddAttribute(string key, string value)
    {
        _attributes.Add((key, value));
        return this;
    }

    public Identity Build() => new(_id, AttributeCollection.FromFlatKeyValuePairs(_attributes));
}
