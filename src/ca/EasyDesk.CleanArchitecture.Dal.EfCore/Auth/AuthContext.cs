using EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auth;

internal class AuthContext : AbstractDbContext
{
    public AuthContext(DbContextOptions<AuthContext> options)
        : base(options)
    {
    }

    public DbSet<TenantModel> Tenants { get; set; }

    public DbSet<IdentityRoleModel> IdentityRoles { get; set; }

    public DbSet<ApiKeyModel> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AuthModel.SchemaName);

        modelBuilder.ApplyConfiguration(new TenantModel.Configuration());
        modelBuilder.ApplyConfiguration(new IdentityRoleModel.Configuration());
        modelBuilder.ApplyConfiguration(new ApiKeyModel.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
