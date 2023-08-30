using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Static;

public record UnknownAgentError(string? Message = null) : Error;
