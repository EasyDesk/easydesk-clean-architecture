namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IEntityPersistence<TDomain, TPersistence> :
    IDomainToPersistence<TDomain, TPersistence>,
    IPersistenceToDomain<TDomain>,
    IMutableEntity<TDomain>
    where TPersistence : IEntityPersistence<TDomain, TPersistence>
{
}
