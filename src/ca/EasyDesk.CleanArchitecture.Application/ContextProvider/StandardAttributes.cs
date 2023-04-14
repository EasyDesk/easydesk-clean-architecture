namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public static class StandardAttributes
{
    public const string FirstName = "firstName";

    public const string LastName = "lastName";

    public const string Email = "email";

    public static Option<string> GetFirstName(this UserInfo userInfo) =>
        userInfo.Attributes.GetSingle(FirstName);

    public static Option<string> GetLastName(this UserInfo userInfo) =>
        userInfo.Attributes.GetSingle(LastName);

    public static Option<string> GetEmail(this UserInfo userInfo) =>
        userInfo.Attributes.GetSingle(Email);
}
