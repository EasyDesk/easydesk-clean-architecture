namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

public interface IDomainPersistence<TDomain>
{
    TDomain ToDomain();
}
