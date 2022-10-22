using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

public class AuthorizationContext : MultitenantDbContext<AuthorizationContext>
{
    public const string SchemaName = "auth";

    public AuthorizationContext(DbContextOptions<AuthorizationContext> options) : base(options)
    {
    }

    public DbSet<UserRoleModel> UserRoles { get; set; }

    public DbSet<RolePermissionModel> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new UserRoleModel.Configuration());
        modelBuilder.ApplyConfiguration(new RolePermissionModel.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
