using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Migrations;

/// <inheritdoc />
public partial class UseHiLoForPet : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "domain",
            incrementBy: 10);

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            schema: "domain",
            table: "Pets",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer")
            .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "domain");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            schema: "domain",
            table: "Pets",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer")
            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
    }
}
