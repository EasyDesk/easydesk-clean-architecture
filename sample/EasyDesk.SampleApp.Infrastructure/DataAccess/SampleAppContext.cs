using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess;

public class SampleAppContext : DomainContext<SampleAppContext>
{
    public DbSet<PersonModel> People { get; set; }

    public DbSet<PetModel> Pets { get; set; }

    public SampleAppContext(ITenantProvider tenantProvider, DbContextOptions<SampleAppContext> options)
        : base(tenantProvider, options)
    {
    }
}
