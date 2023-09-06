namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IMutableEntity<TDomain>
{
    void ApplyChanges(TDomain origin);
}
