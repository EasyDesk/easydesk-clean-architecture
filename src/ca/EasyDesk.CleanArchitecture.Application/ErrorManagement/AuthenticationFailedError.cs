namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record AuthenticationFailedError : ApplicationError
{
    public required string Scheme { get; init; }

    public required string Message { get; init; }

    public override string GetDetail() => $"Authentication with scheme '{Scheme}' failed.";
}
