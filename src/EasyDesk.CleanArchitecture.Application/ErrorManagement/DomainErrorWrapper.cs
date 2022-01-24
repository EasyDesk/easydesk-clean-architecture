using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public record DomainErrorWrapper(DomainError Error) : Error;
