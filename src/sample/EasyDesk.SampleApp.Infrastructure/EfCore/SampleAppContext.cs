using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.EfCore;

public abstract class SampleAppContext : DomainContext
{
    public DbSet<PersonModel> People { get; set; }

    public DbSet<PetModel> Pets { get; set; }

    protected SampleAppContext(DbContextOptions options)
        : base(options)
    {
    }
}

public class PostgreSqlSampleAppContext : SampleAppContext
{
    public PostgreSqlSampleAppContext(DbContextOptions<PostgreSqlSampleAppContext> options)
        : base(options)
    {
    }
}

public class SqlServerSampleAppContext : SampleAppContext
{
    public SqlServerSampleAppContext(DbContextOptions<SqlServerSampleAppContext> options)
        : base(options)
    {
    }
}
