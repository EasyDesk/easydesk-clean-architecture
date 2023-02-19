namespace EasyDesk.CleanArchitecture.Application.Abstractions;

public interface IObjectValue<TApplication, TDomain> : IMappableFrom<TDomain, TApplication>
    where TApplication : IMappableFrom<TDomain, TApplication>
{
    TDomain ToDomainObject();
}
