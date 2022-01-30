using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

public class MultitenantExtension : IDbContextOptionsExtension
{
    private readonly ITenantProvider _tenantProvider;

    public MultitenantExtension(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
        Info = new MultitenantExtensionInfo(this);
    }

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton(_tenantProvider);
        services.AddSingleton<ISaveChangesExtension, SaveChangesExtension>();
        services.AddSingleton<IModelExtension, ModelExtension>();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    public DbContextOptionsExtensionInfo Info { get; }

    private class MultitenantExtensionInfo : DbContextOptionsExtensionInfo
    {
        public MultitenantExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "Multitenant=ON";

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => false;

        public override int GetServiceProviderHashCode() => 0;
    }

    private class SaveChangesExtension : ISaveChangesExtension
    {
        private readonly ITenantProvider _tenantProvider;

        public SaveChangesExtension(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public int Run(DbContext dbContext, Func<int> next)
        {
            SetTenantIdToAddedEntities(dbContext);
            return next();
        }

        public async Task<int> RunAsync(DbContext dbContext, AsyncFunc<int> next, CancellationToken cancellationToken)
        {
            SetTenantIdToAddedEntities(dbContext);
            return await next();
        }

        private void SetTenantIdToAddedEntities(DbContext dbContext)
        {
            var tenantId = _tenantProvider.TenantId.OrElseNull();
            dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .ForEach(e => e.Property(nameof(IMultitenantEntity.TenantId)).CurrentValue = tenantId);
        }
    }

    private class ModelExtension : IModelExtension
    {
        private readonly ITenantProvider _tenantProvider;

        public ModelExtension(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public void Run(ModelBuilder modelBuilder, Action next)
        {
            var entityTypesWithinTenant = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.ClrType.AsOption())
                .Where(t => t.IsAssignableTo(typeof(IMultitenantEntity)))
                .ToList();

            if (entityTypesWithinTenant.Any())
            {
                var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureEntityWithinTenant));
                var args = new object[] { modelBuilder };
                entityTypesWithinTenant
                    .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                    .ForEach(m => m.Invoke(this, args));
            }

            next();
        }

        public void ConfigureEntityWithinTenant<T>(ModelBuilder modelBuilder)
            where T : class, IMultitenantEntity
        {
            var entityBuilder = modelBuilder.Entity<T>();

            entityBuilder.HasIndex(x => x.TenantId);

            entityBuilder.HasQueryFilter(x => x.TenantId == null || x.TenantId == _tenantProvider.TenantId.OrElseNull());
        }
    }
}
