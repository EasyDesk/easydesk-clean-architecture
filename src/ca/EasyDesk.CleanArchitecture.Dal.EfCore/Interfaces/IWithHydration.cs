namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IWithHydration<THydrationData>
{
    THydrationData GetHydrationData();
}
