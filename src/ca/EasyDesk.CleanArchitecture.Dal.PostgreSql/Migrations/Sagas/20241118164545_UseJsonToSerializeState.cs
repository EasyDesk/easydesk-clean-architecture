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
            table: "Sagas",
            newName: "State_Old",
            schema: "sagas");

        migrationBuilder.AddColumn<string>(
            name: "State",
            table: "Sagas",
            type: "text",
            schema: "sagas",
            nullable: false,
            defaultValue: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "State",
            table: "Sagas",
            schema: "sagas");

        migrationBuilder.RenameColumn(
            name: "State_Old",
            table: "Sagas",
            newName: "State",
            schema: "sagas");
    }
}
