using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;

public interface IProjectable<F, T>
    where F : IProjectable<F, T>
{
    private static readonly Lazy<Func<F, T>> _inMemoryMapper = new(() => F.Projection().Compile());

    public static T MapInMemory(F src) => _inMemoryMapper.Value(src);

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
