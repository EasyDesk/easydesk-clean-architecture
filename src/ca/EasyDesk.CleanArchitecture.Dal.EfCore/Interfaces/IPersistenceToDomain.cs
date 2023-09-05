namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IPersistenceToDomain<TDomain>
{
    TDomain ToDomain();
}
