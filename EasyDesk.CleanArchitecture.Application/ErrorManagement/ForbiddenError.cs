namespace EasyDesk.CleanArchitecture.Application.ErrorManagement
{
    public record ForbiddenError(string Reason) : Error(
        $"Cannot perform the requested action: {Reason}",
        Errors.Codes.Forbidden);
}
