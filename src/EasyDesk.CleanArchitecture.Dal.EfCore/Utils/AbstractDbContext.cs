using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class AbstractDbContext<T> : DbContext
    where T : AbstractDbContext<T>
{
    private readonly IList<DbContextExtension> _extensions = new List<DbContextExtension>();

    public AbstractDbContext(ITenantProvider tenantProvider, DbContextOptions<T> options) : base(options)
    {
        this.AddMultitenancy(tenantProvider);
    }

    internal void AddExtension(DbContextExtension extension)
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
