namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record InvalidInputError(string PropertyName, string ErrorMessage) : ApplicationError
{
    public override string GetDetail() => $"Validation for property '{PropertyName}' failed";
}
