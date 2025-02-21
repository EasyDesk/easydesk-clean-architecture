namespace EasyDesk.CleanArchitecture.Application.Authentication;

public record AuthenticationScheme
{
    public required string Scheme { get; init; }

    public required IAuthenticationHandler Handler { get; init; }
}
