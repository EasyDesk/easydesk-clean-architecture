using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Abstractions;

public interface ISnapshot<TApplication, TDomain> : IMappableFrom<TDomain, TApplication>
    where TApplication : IMappableFrom<TDomain, TApplication>
    where TDomain : Entity;
