using System;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record NotFoundError(Type EntityType) : Error;
