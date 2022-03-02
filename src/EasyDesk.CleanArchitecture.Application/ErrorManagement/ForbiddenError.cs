using EasyDesk.Tools.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record ForbiddenError(string Reason) : Error;
