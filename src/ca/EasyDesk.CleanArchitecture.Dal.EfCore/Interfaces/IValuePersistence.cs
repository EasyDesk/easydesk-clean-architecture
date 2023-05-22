using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;

public interface IValuePersistence<TDomain, TPersistence, TDomainParent>
    : IMutablePersistence<TDomain>,
    IPersistenceModel<TDomainParent, TPersistence>
    where TPersistence : IMutablePersistence<TDomain>, IPersistenceModel<TDomainParent, TPersistence>
{
}
