using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IEntityPersistence<TDomain, TPersistence>
    : IMutablePersistence<TDomain>,
    IPersistenceModel<TDomain, TPersistence>
    where TPersistence : IMutablePersistence<TDomain>,
    IPersistenceModel<TDomain, TPersistence>
{
}
