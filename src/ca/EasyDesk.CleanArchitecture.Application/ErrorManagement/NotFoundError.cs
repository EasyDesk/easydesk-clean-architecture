namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record NotFoundError : ApplicationError
{
    public override string GetDetail() => "Unable to find the requested resource";
}
