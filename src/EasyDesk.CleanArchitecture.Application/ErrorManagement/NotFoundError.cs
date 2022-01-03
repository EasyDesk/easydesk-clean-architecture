using System;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record NotFoundError(Type EntityType) : Error(
    $"Unable to find entity of type {{{EntityType.Name}}}",
    Errors.Codes.NotFound);
