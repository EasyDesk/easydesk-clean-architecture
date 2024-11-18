using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Sagas;

/// <inheritdoc />
public partial class UseJsonToSerializeState : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "State",
            schema: "sagas",
            table: "Sagas",
            newName: "State_Old");

        migrationBuilder.AddColumn<string>(
            name: "State",
            schema: "sagas",
            table: "Sagas",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "State",
            schema: "sagas",
            table: "Sagas");

        migrationBuilder.RenameColumn(
            name: "State_Old",
            schema: "sagas",
            table: "Sagas",
            newName: "State");
    }
}
