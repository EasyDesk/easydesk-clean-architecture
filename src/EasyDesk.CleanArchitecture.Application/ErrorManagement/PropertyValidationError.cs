namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record PropertyValidationError(string PropertyName, string ErrorMessage) : Error;
