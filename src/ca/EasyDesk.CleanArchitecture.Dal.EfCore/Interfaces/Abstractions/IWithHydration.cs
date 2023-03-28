namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

public interface IWithHydration<THydrationData>
{
    THydrationData GetHydrationData();
}
