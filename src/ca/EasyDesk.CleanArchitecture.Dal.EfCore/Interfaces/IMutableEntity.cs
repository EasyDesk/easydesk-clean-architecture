namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IMutableEntity<TDomain, TPersistence>
    where TPersistence : IMutableEntity<TDomain, TPersistence>
{
    void ApplyChanges(TDomain origin);
}
