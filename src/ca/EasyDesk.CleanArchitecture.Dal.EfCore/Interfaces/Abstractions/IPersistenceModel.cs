namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

public interface IPersistenceModel<TDomain, TPersistence>
    where TPersistence : IPersistenceModel<TDomain, TPersistence>
{
    static abstract TPersistence ToPersistence(TDomain origin);
}
