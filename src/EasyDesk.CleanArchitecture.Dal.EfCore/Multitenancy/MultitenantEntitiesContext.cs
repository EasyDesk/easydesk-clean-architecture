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
        private readonly ITenantProvider _tenantService;

        protected MultitenantEntitiesContext(ITenantProvider tenantService)
        {
            _tenantService = tenantService;
        }

        protected MultitenantEntitiesContext(ITenantProvider tenantService, DbContextOptions options) : base(options)
        {
            _tenantService = tenantService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.ClrType.AsOption())
                .Where(t => t.IsAssignableTo(typeof(IEntityWithinTenant)))
                .ForEach(t =>
                {
                    var method = GetType().GetMethod(nameof(HasEntityWithinTenant)).MakeGenericMethod(t);
                    method.Invoke(this, new object[] { modelBuilder });
                });

            base.OnModelCreating(modelBuilder);
        }

        public void HasEntityWithinTenant<T>(ModelBuilder modelBuilder)
            where T : class, IEntityWithinTenant
        {
            var entityBuilder = modelBuilder.Entity<T>();
            entityBuilder.Property(x => x.TenantId)
                .IsRequired();
            entityBuilder.HasQueryFilter(x => x.TenantId == _tenantService.TenantId.OrElseNull());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantIdToTrackedEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantIdToTrackedEntities()
        {
            _tenantService.TenantId.IfPresent(tenantId =>
            {
                ChangeTracker.Entries<IEntityWithinTenant>()
                .ForEach(entry => entry.Entity.TenantId = tenantId);
            });
        }
    }
}
