using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy
{
    public abstract class MultitenantEntitiesContext : EntitiesContext
    {
        private const string TenantIdProperty = "TenantId";

        private readonly ITenantProvider _tenantProvider;

        protected MultitenantEntitiesContext(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        protected MultitenantEntitiesContext(ITenantProvider tenantService, DbContextOptions options) : base(options)
        {
            _tenantProvider = tenantService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
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

            base.OnModelCreating(modelBuilder);
        }

        public void ConfigureEntityWithinTenant<T>(ModelBuilder modelBuilder)
            where T : class
        {
            var entityBuilder = modelBuilder.Entity<T>();

            entityBuilder.Property<string>(TenantIdProperty)
                .IsRequired();

            entityBuilder.HasQueryFilter(x => EF.Property<string>(x, TenantIdProperty) == _tenantProvider.TenantId.OrElseNull());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantIdToAddedEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantIdToAddedEntities()
        {
            _tenantProvider.TenantId.IfPresent(tenantId =>
            {
                ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added)
                    .ForEach(e => e.Property(TenantIdProperty).CurrentValue = tenantId);
            });
        }
    }
}
