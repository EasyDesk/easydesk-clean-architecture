﻿// <auto-generated />
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Authorization;

[DbContext(typeof(AuthorizationContext))]
[Migration("20221110160350_AddMultitenantSupport")]
partial class AddMultitenantSupport
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("auth")
            .HasAnnotation("ProductVersion", "6.0.7")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model.RolePermissionModel", b =>
            {
                b.Property<string>("RoleId")
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)");

                b.Property<string>("PermissionName")
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)");

                b.Property<string>("TenantId")
                    .HasColumnType("text");

                b.HasKey("RoleId", "PermissionName");

                b.HasIndex("TenantId");

                b.ToTable("RolePermissions", "auth");
            });

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model.UserRoleModel", b =>
            {
                b.Property<string>("UserId")
                    .HasColumnType("text");

                b.Property<string>("RoleId")
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)");

                b.Property<string>("TenantId")
                    .HasColumnType("text");

                b.HasKey("UserId", "RoleId");

                b.HasIndex("TenantId");

                b.ToTable("UserRoles", "auth");
            });
#pragma warning restore 612, 618
    }
}