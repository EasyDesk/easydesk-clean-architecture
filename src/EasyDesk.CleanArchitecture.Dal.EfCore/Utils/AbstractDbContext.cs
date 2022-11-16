using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class AbstractDbContext<T> : DbContext
    where T : AbstractDbContext<T>
{
    private readonly IList<DbContextExtension> _extensions = new List<DbContextExtension>();

    public AbstractDbContext(DbContextOptions<T> options) : base(options)
    {
    }

    internal void AddExtension(DbContextExtension extension)
    {
        extension.Initialize(this);
        _extensions.Add(extension);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var queryFiltersBuilder = new QueryFiltersBuilder();
        _extensions.Aggregate(
            () => base.OnModelCreating(modelBuilder),
            (curr, ext) => () => ext.ConfigureModel(modelBuilder, queryFiltersBuilder, curr))();
        queryFiltersBuilder.ApplyToModelBuilder(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _extensions.Aggregate(
            () => base.SaveChangesAsync(cancellationToken),
            (curr, ext) => () => ext.SaveChanges(curr))();
    }
}
