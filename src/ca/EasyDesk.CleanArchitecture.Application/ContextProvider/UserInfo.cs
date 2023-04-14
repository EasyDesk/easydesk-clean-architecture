namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record UserInfo(UserId UserId, AttributeCollection Attributes)
{
    public static UserInfo Create(UserId userId) =>
        Create(userId, AttributeCollection.Empty);

    public static UserInfo Create(UserId userId, AttributeCollection attributes) =>
        new(userId, attributes);
}
