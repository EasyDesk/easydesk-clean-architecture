using EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public abstract class ExtendedDbContext : DbContext
{
    public ExtendedDbContext(DbContextOptions options) : base(options)
    {
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        void LastStep()
        {
            SetupModel(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }
        this.GetService<IEnumerable<IModelExtension>>().FoldRight(LastStep, (ext, curr) => () => ext.Run(modelBuilder, curr))();
    }

    protected virtual void SetupModel(ModelBuilder modelBuilder)
    {
    }

    public override int SaveChanges()
    {
        return this.GetService<IEnumerable<ISaveChangesExtension>>()
            .FoldRight(() => base.SaveChanges(), (ext, curr) => () => ext.Run(this, curr))();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await this.GetService<IEnumerable<ISaveChangesExtension>>()
            .FoldRight(() => base.SaveChangesAsync(), (ext, curr) => () => ext.RunAsync(this, () => curr(), cancellationToken))();
    }
}
