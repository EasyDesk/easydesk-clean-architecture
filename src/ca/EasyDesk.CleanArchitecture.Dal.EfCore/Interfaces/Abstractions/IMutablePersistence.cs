namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

public interface IMutablePersistence<TDomain, TPersistence>
    : IDomainPersistence<TDomain>
    where TPersistence : IMutablePersistence<TDomain, TPersistence>
{
    static abstract void ApplyChanges(TDomain origin, TPersistence destination);
}
