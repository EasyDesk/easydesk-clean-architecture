﻿// <auto-generated />
using EasyDesk.CleanArchitecture.Dal.EfCore.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auth;

[DbContext(typeof(AuthContext))]
[Migration("20240927150321_AddApiKeys")]
partial class AddApiKeys
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("auth")
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.ApiKeyModel", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint");

                NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                b.Property<string>("ApiKey")
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnType("character varying(128)");

                b.HasKey("Id");

                b.HasIndex("ApiKey")
                    .IsUnique();

                b.ToTable("ApiKeys", "auth");
            });

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.IdentityRoleModel", b =>
            {
                b.Property<string>("Identity")
                    .HasMaxLength(1024)
                    .HasColumnType("character varying(1024)");

                b.Property<string>("Role")
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)");

                b.Property<string>("Tenant")
                    .ValueGeneratedOnAdd()
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("Realm")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("TenantFk")
                    .ValueGeneratedOnAdd()
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.HasKey("Identity", "Role", "Tenant");

                b.HasIndex("Tenant");

                b.HasIndex("TenantFk");

                b.ToTable("IdentityRoles", "auth");
            });

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.TenantModel", b =>
            {
                b.Property<string>("Id")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.HasKey("Id");

                b.ToTable("Tenants", "auth");
            });

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.ApiKeyModel", b =>
            {
                b.OwnsMany("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.ApiKeyIdentityModel", "Identities", b1 =>
                    {
                        b1.Property<long>("Id")
                            .ValueGeneratedOnAdd()
                            .HasColumnType("bigint");

                        NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<long>("Id"));

                        b1.Property<long>("ApiKeyId")
                            .HasColumnType("bigint");

                        b1.Property<string>("IdentityId")
                            .IsRequired()
                            .HasColumnType("text");

                        b1.Property<string>("IdentityRealm")
                            .IsRequired()
                            .HasColumnType("text");

                        b1.HasKey("Id");

                        b1.HasIndex("ApiKeyId");

                        b1.ToTable("ApiKeyIdentityModel", "auth");

                        b1.WithOwner()
                            .HasForeignKey("ApiKeyId");

                        b1.OwnsMany("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.ApiKeyIdentityAttributeModel", "Attributes", b2 =>
                            {
                                b2.Property<long>("Id")
                                    .ValueGeneratedOnAdd()
                                    .HasColumnType("bigint");

                                NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b2.Property<long>("Id"));

                                b2.Property<string>("AttributeName")
                                    .IsRequired()
                                    .HasColumnType("text");

                                b2.Property<string>("AttributeValue")
                                    .IsRequired()
                                    .HasColumnType("text");

                                b2.Property<long>("IdentityId")
                                    .HasColumnType("bigint");

                                b2.HasKey("Id");

                                b2.HasIndex("IdentityId");

                                b2.ToTable("ApiKeyIdentityAttributeModel", "auth");

                                b2.WithOwner()
                                    .HasForeignKey("IdentityId");
                            });

                        b1.Navigation("Attributes");
                    });

                b.Navigation("Identities");
            });

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.IdentityRoleModel", b =>
            {
                b.HasOne("EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model.TenantModel", null)
                    .WithMany()
                    .HasForeignKey("TenantFk")
                    .OnDelete(DeleteBehavior.Cascade);
            });
#pragma warning restore 612, 618
    }
}