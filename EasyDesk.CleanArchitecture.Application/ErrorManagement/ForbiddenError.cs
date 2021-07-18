namespace EasyDesk.CleanArchitecture.Application.ErrorManagement
{
    public record ForbiddenError(string Reason) : Error(
        $"Cannot access the requested resource: {Reason}",
        Errors.Codes.Forbidden);
}
