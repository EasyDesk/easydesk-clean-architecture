using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public static class StandardAttributes
{
    public const string FirstName = "firstName";

    public const string LastName = "lastName";

    public const string Email = "email";

    public static Option<string> GetFirstName(this Identity identity) =>
        identity.Attributes.GetSingle(FirstName);

    public static Option<string> GetLastName(this Identity identity) =>
        identity.Attributes.GetSingle(LastName);

    public static Option<string> GetEmail(this Identity identity) =>
        identity.Attributes.GetSingle(Email);
}
