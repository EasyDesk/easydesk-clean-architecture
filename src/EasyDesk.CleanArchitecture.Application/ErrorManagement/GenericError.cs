using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record GenericError(string Message, string ErrorCode, IImmutableDictionary<string, object> Parameters) : Error;
