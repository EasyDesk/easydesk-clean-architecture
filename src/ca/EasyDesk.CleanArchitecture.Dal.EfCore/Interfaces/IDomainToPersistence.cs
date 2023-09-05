namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IDomainToPersistence<TDomain, TPersistence>
    where TPersistence : IDomainToPersistence<TDomain, TPersistence>
{
    static abstract TPersistence ToPersistence(TDomain origin);
}
