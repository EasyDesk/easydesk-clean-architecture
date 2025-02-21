namespace EasyDesk.CleanArchitecture.Application.Authentication;

public class IdentityBuilder
{
    private readonly Realm _realm;
    private readonly IdentityId _id;
    private readonly ISet<(string, string)> _attributes = new HashSet<(string, string)>();

    public IdentityBuilder(Realm realm, IdentityId id)
    {
        _realm = realm;
        _id = id;
    }

    public IdentityBuilder AddAttribute(string key, string value)
    {
        _attributes.Add((key, value));
        return this;
    }

    public Identity Build() => new(_realm, _id, AttributeCollection.FromFlatKeyValuePairs(_attributes));
}
