namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record Identity(IdentityId Id, AttributeCollection Attributes)
{
    public Identity(IdentityId id) : this(id, AttributeCollection.Empty)
    {
    }
}
