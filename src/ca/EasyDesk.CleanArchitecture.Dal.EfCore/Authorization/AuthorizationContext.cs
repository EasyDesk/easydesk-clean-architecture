using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

internal class AuthorizationContext : AbstractDbContext<AuthorizationContext>
{
    public AuthorizationContext(DbContextOptions<AuthorizationContext> options)
        : base(options)
    {
    }

    public DbSet<TenantModel> Tenants { get; set; }

    public DbSet<UserRoleModel> UserRoles { get; set; }

    public DbSet<RolePermissionModel> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AuthorizationModel.SchemaName);

        modelBuilder.ApplyConfiguration(new TenantModel.Configuration());
        modelBuilder.ApplyConfiguration(new UserRoleModel.Configuration());
        modelBuilder.ApplyConfiguration(new RolePermissionModel.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
