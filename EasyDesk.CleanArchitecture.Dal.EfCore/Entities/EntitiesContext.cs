using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Entities
{
    public abstract class EntitiesContext : DbContext
    {
        public const string SchemaName = "entities";

        protected EntitiesContext()
        {
        }

        protected EntitiesContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SchemaName);

            base.OnModelCreating(modelBuilder);
        }
    }
}
