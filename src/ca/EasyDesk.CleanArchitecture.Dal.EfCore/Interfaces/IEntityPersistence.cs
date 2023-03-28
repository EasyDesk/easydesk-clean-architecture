using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;

public interface IEntityPersistence<TDomain, TPersistence>
    : IMutablePersistence<TDomain, TPersistence>,
    IPersistenceModel<TDomain, TPersistence>
    where TPersistence : IMutablePersistence<TDomain, TPersistence>,
    IPersistenceModel<TDomain, TPersistence>
{
}
