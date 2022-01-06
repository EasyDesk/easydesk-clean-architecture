using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Domain.Model;

namespace EasyDesk.CleanArchitecture.Web.Mapping;

public class MapStringToEmail : DirectMapping<string, Email>
{
    public MapStringToEmail() : base(e => Email.From(e))
    {
    }
}

public class MapStringToName : DirectMapping<string, Name>
{
    public MapStringToName() : base(e => Name.From(e))
    {
    }
}

public class MapStringToTokenValue : DirectMapping<string, Token>
{
    public MapStringToTokenValue() : base(e => Token.From(e))
    {
    }
}
