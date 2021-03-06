// <auto-generated />
using EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence.Migrations
{
    [DbContext(typeof(IdempotenceContext))]
    [Migration("20220129011807_InitialSchema")]
    partial class InitialSchema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("idempotence")
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence.HandledMessage", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("HandledMessages", "idempotence");
                });
#pragma warning restore 612, 618
        }
    }
}
