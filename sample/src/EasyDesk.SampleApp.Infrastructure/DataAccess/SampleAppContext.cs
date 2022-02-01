using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess;

public class SampleAppContext : DomainContext
{
    public DbSet<PersonModel> People { get; set; }

    public SampleAppContext(DbContextOptions<SampleAppContext> options) : base(options)
    {
    }
}
