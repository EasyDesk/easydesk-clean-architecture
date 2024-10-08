﻿// <auto-generated />
using EasyDesk.CleanArchitecture.Dal.EfCore.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Auth;

[DbContext(typeof(AuthContext))]
[Migration("20240409131409_RemovePermissions")]
partial class RemovePermissions
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("auth")
            .HasAnnotation("ProductVersion", "8.0.3")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model.IdentityRoleModel", b =>
            {
                b.Property<string>("Identity")
                    .HasMaxLength(1024)
                    .HasColumnType("nvarchar(1024)");

                b.Property<string>("Role")
                    .HasMaxLength(100)
                    .HasColumnType("nvarchar(100)");

                b.Property<string>("Tenant")
                    .ValueGeneratedOnAdd()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("Realm")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("TenantFk")
                    .ValueGeneratedOnAdd()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.HasKey("Identity", "Role", "Tenant");

                b.HasIndex("Tenant");

                b.HasIndex("TenantFk");

                b.ToTable("IdentityRoles", "auth");
            });

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model.TenantModel", b =>
            {
                b.Property<string>("Id")
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.HasKey("Id");

                b.ToTable("Tenants", "auth");
            });

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model.IdentityRoleModel", b =>
            {
                b.HasOne("EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model.TenantModel", null)
                    .WithMany()
                    .HasForeignKey("TenantFk")
                    .OnDelete(DeleteBehavior.Cascade);
            });
#pragma warning restore 612, 618
    }
}
