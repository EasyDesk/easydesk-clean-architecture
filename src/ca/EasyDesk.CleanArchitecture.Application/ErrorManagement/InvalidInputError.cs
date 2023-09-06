using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record InvalidInputError(string PropertyName, string ErrorCode, string ErrorMessage, IImmutableDictionary<string, object> Parameters) : ApplicationError
{
    public override string GetDetail() => $"Validation for property '{PropertyName}' failed";
}
