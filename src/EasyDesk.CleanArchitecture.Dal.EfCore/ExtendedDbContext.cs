using EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public abstract class ExtendedDbContext : DbContext
{
    public ExtendedDbContext(DbContextOptions options) : base(options)
    {
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder) =>
        this.GetService<ModelExtensionsRunner>().Run(modelBuilder, builder =>
        {
            SetupModel(builder);
            base.OnModelCreating(builder);
        });

    protected virtual void SetupModel(ModelBuilder modelBuilder)
    {
    }

    public override int SaveChanges() =>
        this.GetService<SaveChangesExtensionsRunner>().Run(this);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        this.GetService<SaveChangesExtensionsRunner>().RunAsync(this, cancellationToken);
}
