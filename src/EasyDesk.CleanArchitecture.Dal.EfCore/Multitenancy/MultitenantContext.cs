namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

////public abstract class MultitenantContext : DbContext
////{
////    private const string TenantIdProperty = "TenantId";

////    private readonly ITenantProvider _tenantProvider;

////    protected MultitenantContext(DbContextOptions options) : base(options)
////    {
////    }

////    protected override void OnModelCreating(ModelBuilder modelBuilder)
////    {
////        var entityTypesWithinTenant = modelBuilder.Model
////            .GetEntityTypes()
////            .SelectMany(e => e.ClrType.AsOption())
////            .ToList();

////        if (entityTypesWithinTenant.Any())
////        {
////            var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureEntityWithinTenant));
////            var args = new object[] { modelBuilder };
////            entityTypesWithinTenant
////                .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
////                .ForEach(m => m.Invoke(this, args));
////        }

////        base.OnModelCreating(modelBuilder);
////    }

////    public void ConfigureEntityWithinTenant<T>(ModelBuilder modelBuilder)
////        where T : class
////    {
////        var entityBuilder = modelBuilder.Entity<T>();

////        entityBuilder.Property<string>(TenantIdProperty)
////            .IsRequired();

////        entityBuilder.HasIndex(TenantIdProperty);

////        entityBuilder.HasQueryFilter(x => EF.Property<string>(x, TenantIdProperty) == _tenantProvider.TenantId.OrElseNull());
////    }

////    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
////    {
////        SetTenantIdToAddedEntities();
////        return base.SaveChangesAsync(cancellationToken);
////    }

////    public override int SaveChanges()
////    {
////        SetTenantIdToAddedEntities();
////        return base.SaveChanges();
////    }

////    private void SetTenantIdToAddedEntities()
////    {
////        _tenantProvider.TenantId.IfPresent(tenantId =>
////        {
////            ChangeTracker.Entries()
////                .Where(e => e.State == EntityState.Added)
////                .ForEach(e => e.Property(TenantIdProperty).CurrentValue = tenantId);
////        });
////    }
////}
