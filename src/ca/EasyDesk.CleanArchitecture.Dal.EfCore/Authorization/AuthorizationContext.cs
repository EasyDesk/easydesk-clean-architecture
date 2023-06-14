using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

internal class AuthorizationContext : AbstractDbContext
{
    public AuthorizationContext(DbContextOptions<AuthorizationContext> options)
        : base(options)
    {
    }

    public DbSet<TenantModel> Tenants { get; set; }

    public DbSet<IdentityRoleModel> IdentityRoles { get; set; }

    public DbSet<RolePermissionModel> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AuthorizationModel.SchemaName);

        modelBuilder.ApplyConfiguration(new TenantModel.Configuration());
        modelBuilder.ApplyConfiguration(new IdentityRoleModel.Configuration());
        modelBuilder.ApplyConfiguration(new RolePermissionModel.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
