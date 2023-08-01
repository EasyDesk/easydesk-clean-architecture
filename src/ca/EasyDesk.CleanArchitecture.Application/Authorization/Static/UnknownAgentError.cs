namespace EasyDesk.CleanArchitecture.Application.Authorization;

public record UnknownAgentError(string? Message = null) : Error;
