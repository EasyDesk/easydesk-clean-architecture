using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record InputValidationError(string PropertyName, string ErrorMessage) : Error;
