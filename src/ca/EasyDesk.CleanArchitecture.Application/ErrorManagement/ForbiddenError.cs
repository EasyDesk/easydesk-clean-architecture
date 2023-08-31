namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record ForbiddenError(string Reason) : ApplicationError
{
    public override string GetDetail() => "Missing permissions to access requested resources";
}
