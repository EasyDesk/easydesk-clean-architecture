using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public abstract record Error;

public record MultipleErrors(IImmutableList<Error> Errors) : Error;
