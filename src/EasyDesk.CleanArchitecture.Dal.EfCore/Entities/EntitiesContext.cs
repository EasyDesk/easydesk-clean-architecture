using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Entities;

public abstract class EntitiesContext : ExtendedDbContext
{
    public const string SchemaName = "entities";

    protected EntitiesContext(DbContextOptions options) : base(options)
    {
    }

    protected override void SetupModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}
