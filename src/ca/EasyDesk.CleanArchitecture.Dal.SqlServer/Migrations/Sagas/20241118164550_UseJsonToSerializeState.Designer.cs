﻿// <auto-generated />
using System;
using EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Sagas;

[DbContext(typeof(SagasContext))]
[Migration("20241118164550_UseJsonToSerializeState")]
partial class UseJsonToSerializeState
{
    /// <inheritdoc />
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("sagas")
            .HasAnnotation("ProductVersion", "8.0.10")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Sagas.SagaModel", b =>
            {
                b.Property<string>("Id")
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Type")
                    .HasMaxLength(2048)
                    .HasColumnType("nvarchar(2048)");

                b.Property<string>("Tenant")
                    .ValueGeneratedOnAdd()
                    .HasMaxLength(256)
                    .HasColumnType("nvarchar(256)");

                b.Property<string>("State")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<byte[]>("State_Old")
                    .IsRequired()
                    .HasColumnType("varbinary(max)");

                b.Property<int?>("Version")
                    .HasColumnType("int");

                b.HasKey("Id", "Type", "Tenant");

                b.HasIndex("Tenant");

                b.ToTable("Sagas", "sagas");
            });
#pragma warning restore 612, 618
    }
}