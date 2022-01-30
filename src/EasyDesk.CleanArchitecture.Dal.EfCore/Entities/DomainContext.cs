using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Entities;

public abstract class DomainContext : ExtendedDbContext
{
    public const string SchemaName = "domain";

    protected DomainContext(DbContextOptions options) : base(options)
    {
    }

    protected override void SetupModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}
