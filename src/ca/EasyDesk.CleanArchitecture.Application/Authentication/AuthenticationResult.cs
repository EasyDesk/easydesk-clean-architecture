namespace EasyDesk.CleanArchitecture.Application.Authentication;

public abstract record AuthenticationResult
{
    private AuthenticationResult()
    {
    }

    public record Failed : AuthenticationResult
    {
        public required string ErrorMessage { get; init; }
    }

    public record Authenticated : AuthenticationResult
    {
        public required Agent Agent { get; init; }
    }
}
