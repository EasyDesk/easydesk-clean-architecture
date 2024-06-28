using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Domain;

public static class AggregateVersioningUtils
{
    public const string VersionPropertyName = "_Version";

    public static void IncrementVersion<T>(this EntityEntry<T> entry) where T : class, IAggregateRootModel
    {
        entry.Property<long>(VersionPropertyName).CurrentValue++;
    }
}
