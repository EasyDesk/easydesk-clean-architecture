namespace EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;

public interface IProjectable<F, T>
    where F : IProjectable<F, T>
{
    static abstract IQueryable<T> Projection(IQueryable<F> query);
}

public static class ProjectionExtensions
{
    public static IQueryable<T> Project<F, T>(this IQueryable<F> query)
        where F : IProjectable<F, T>
    {
        return F.Projection(query);
    }
}
