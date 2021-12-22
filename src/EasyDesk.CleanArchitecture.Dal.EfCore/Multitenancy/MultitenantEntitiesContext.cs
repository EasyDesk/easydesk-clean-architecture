using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
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
