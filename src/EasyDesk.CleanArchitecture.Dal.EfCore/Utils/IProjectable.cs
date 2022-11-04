using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public interface IProjectable<F, T>
    where F : IProjectable<F, T>
{
    static abstract Expression<Func<F, T>> Projection();
}

public static class ProjectionExtensions
{
    public static IQueryable<T> Project<F, T>(this IQueryable<F> query)
        where F : IProjectable<F, T>
    {
        return query.Select(F.Projection());
    }
}
