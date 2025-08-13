using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.SqlServer;

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
            type: "int",
            schema: "domain",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int")
            .Annotation("SqlServer:Identity", "1, 1");
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
            type: "int",
            schema: "domain",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int")
            .OldAnnotation("SqlServer:Identity", "1, 1");
    }
}
