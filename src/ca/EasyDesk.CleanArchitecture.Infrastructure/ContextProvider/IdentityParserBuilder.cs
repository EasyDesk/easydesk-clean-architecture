using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public class IdentityParserBuilder
{
    private readonly ClaimsPrincipalParser<IdentityId> _id;
    private readonly List<(string Name, ClaimsPrincipalParser<string> Parser)> _attributes = new();

    public IdentityParserBuilder(ClaimsPrincipalParser<IdentityId> id)
    {
        _id = id;
    }

    public IdentityParserBuilder WithAttribute(string name, ClaimsPrincipalParser<string> parser)
    {
        _attributes.Add((name, parser));
        return this;
    }

    public IdentityParserBuilder WithAttribute(string name, string claimName) =>
        WithAttribute(name, ClaimsPrincipalParsers.ForClaim(claimName));

    public ClaimsPrincipalParser<Identity> Build() => claimsPrincipal =>
    {
        return _id(claimsPrincipal).Map(i =>
        {
            var flatAttributes = _attributes.SelectMany(a => a.Parser(claimsPrincipal).Map(x => (a.Name, x)));
            return new Identity(i, AttributeCollection.FromFlatKeyValuePairs(flatAttributes));
        });
    };
}
