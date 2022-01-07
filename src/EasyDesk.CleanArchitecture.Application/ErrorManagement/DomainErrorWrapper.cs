using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record DomainErrorWrapper(DomainError Error) : Error(
    $"Domain Error: {Error.GetType().Name}",
    Error.GetType().Name);

public record DomainErrorsWrapper(IEnumerable<DomainError> Errors) : Error(
    $"Domain Errors: {Errors.Select(e => e.GetType().Name).ConcatStrings(", ")}",
    Errors.Select(e => e.GetType().Name).ConcatStrings(";"));
