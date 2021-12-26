using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess
{
    public class SampleAppContext : MultitenantEntitiesContext
    {
        public DbSet<PersonModel> People { get; set; }

        public SampleAppContext(ITenantProvider tenantProvider, DbContextOptions<SampleAppContext> options)
            : base(tenantProvider, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleAppContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
