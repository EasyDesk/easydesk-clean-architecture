namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record UserInfo(UserId UserId, AttributeCollection Attributes)
{
    public UserInfo(UserId userId) : this(userId, AttributeCollection.Empty)
    {
    }
}
