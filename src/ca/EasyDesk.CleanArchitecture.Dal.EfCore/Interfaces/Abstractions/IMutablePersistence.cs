namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

public interface IMutablePersistence<TDomain>
    : IDomainPersistence<TDomain>
{
    void ApplyChanges(TDomain origin);
}
