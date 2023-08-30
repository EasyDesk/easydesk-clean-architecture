using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record ForbiddenError(string Reason) : Error;
