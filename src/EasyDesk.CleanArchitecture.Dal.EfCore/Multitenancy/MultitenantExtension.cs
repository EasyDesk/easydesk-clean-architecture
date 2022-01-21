using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

public class MultitenantExtension : IDbContextOptionsExtension
{
    private const string TenantIdProperty = "TenantId";

    private readonly ITenantProvider _tenantProvider;

    public MultitenantExtension(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
        Info = new MultitenantExtensionInfo(this);
    }

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton(_tenantProvider);
        services.TryAddSingleton<ModelExtensionsRunner>();
        services.TryAddSingleton<SaveChangesExtensionsRunner>();
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

        public async Task<int> RunAsync(DbContext dbContext, AsyncFunc<CancellationToken, int> next, CancellationToken cancellationToken)
        {
            SetTenantIdToAddedEntities(dbContext);
            return await next(cancellationToken);
        }

        private void SetTenantIdToAddedEntities(DbContext dbContext)
        {
            _tenantProvider.TenantId.IfPresent(tenantId =>
            {
                dbContext.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added)
                    .ForEach(e => e.Property(TenantIdProperty).CurrentValue = tenantId);
            });
        }
    }

    private class ModelExtension : IModelExtension
    {
        private readonly ITenantProvider _tenantProvider;

        public ModelExtension(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public void Run(ModelBuilder modelBuilder, Action<ModelBuilder> next)
        {
            var entityTypesWithinTenant = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.ClrType.AsOption())
                .ToList();

            if (entityTypesWithinTenant.Any())
            {
                var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureEntityWithinTenant));
                var args = new object[] { modelBuilder };
                entityTypesWithinTenant
                    .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                    .ForEach(m => m.Invoke(this, args));
            }

            next(modelBuilder);
        }

        public void ConfigureEntityWithinTenant<T>(ModelBuilder modelBuilder)
            where T : class
        {
            var entityBuilder = modelBuilder.Entity<T>();

            entityBuilder.Property<string>(TenantIdProperty)
                .IsRequired();

            entityBuilder.HasIndex(TenantIdProperty);

            entityBuilder.HasQueryFilter(x => EF.Property<string>(x, TenantIdProperty) == _tenantProvider.TenantId.OrElseNull());
        }
    }
}
