namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record InternalError(Exception Exception) : Error;
