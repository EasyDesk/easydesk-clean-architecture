using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public interface IMapFromDomainError<A, D> : IMappableFrom<D, A>
    where A : ApplicationError, IMapFromDomainError<A, D>
    where D : DomainError;
