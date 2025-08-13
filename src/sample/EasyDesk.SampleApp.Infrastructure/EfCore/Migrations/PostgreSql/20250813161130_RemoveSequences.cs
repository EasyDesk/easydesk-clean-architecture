using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.PostgreSql;

/// <inheritdoc />
public partial class RemoveSequences : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "domain");

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Pets",
            type: "integer",
            schema: "domain",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer")
            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "domain",
            incrementBy: 10);

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "Pets",
            type: "integer",
            schema: "domain",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer")
            .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
    }
}
