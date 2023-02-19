namespace EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;

public interface IDomainPersistence<TDomain, TPersistence>
    where TPersistence : IDomainPersistence<TDomain, TPersistence>
{
    TDomain ToDomain();

    static abstract TPersistence ToPersistence(TDomain origin);
}
