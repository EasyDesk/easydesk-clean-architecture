using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;

public class AuthorizationContext : ExtendedDbContext
{
    public const string SchemaName = "auth";

    public AuthorizationContext(DbContextOptions<AuthorizationContext> options) : base(options)
    {
    }

    public DbSet<UserRoleModel> UserRoles { get; set; }

    public DbSet<RolePermissionModel> RolePermissions { get; set; }

    protected override void SetupModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new UserRoleModel.Configuration());
        modelBuilder.ApplyConfiguration(new RolePermissionModel.Configuration());
    }
}
