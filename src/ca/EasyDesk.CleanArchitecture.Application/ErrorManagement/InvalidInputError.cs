using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record InvalidInputError(string PropertyName, string ErrorCode, string ErrorMessage, IFixedMap<string, object?> Parameters) : ApplicationError
{
    public override string GetDetail() => $"Validation for property '{PropertyName}' failed";
}
