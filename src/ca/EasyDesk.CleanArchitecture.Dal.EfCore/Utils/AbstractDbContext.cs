using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class AbstractDbContext : DbContext
{
    private readonly IList<DbContextExtension> _extensions = [];

    public AbstractDbContext(DbContextOptions options) : base(options)
    {
    }

    protected void AddExtension(DbContextExtension extension)
    {
        extension.Initialize(this);
        _extensions.Add(extension);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var queryFilters = new QueryFiltersBuilder();
        _extensions.Aggregate(
            () => base.OnModelCreating(modelBuilder),
            (curr, ext) => () => ext.ConfigureModel(modelBuilder, queryFilters, curr))();

        queryFilters.ApplyToModelBuilder(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _extensions.Aggregate(
            () => base.SaveChangesAsync(cancellationToken),
            (curr, ext) => () => ext.SaveChanges(curr))();
    }
}
