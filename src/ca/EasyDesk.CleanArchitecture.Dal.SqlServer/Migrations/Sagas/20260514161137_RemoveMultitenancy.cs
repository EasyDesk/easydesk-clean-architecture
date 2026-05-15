using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Sagas;

/// <inheritdoc />
public partial class RemoveMultitenancy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_Sagas",
            table: "Sagas",
            schema: "sagas");

        migrationBuilder.DropIndex(
            name: "IX_Sagas_Tenant",
            table: "Sagas",
            schema: "sagas");

        migrationBuilder.DropColumn(
            name: "Tenant",
            table: "Sagas",
            schema: "sagas");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Sagas",
            table: "Sagas",
            columns: new[] { "Id", "Type", },
            schema: "sagas");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_Sagas",
            table: "Sagas",
            schema: "sagas");

        migrationBuilder.AddColumn<string>(
            name: "Tenant",
            table: "Sagas",
            type: "nvarchar(256)",
            maxLength: 256,
            schema: "sagas",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddPrimaryKey(
            name: "PK_Sagas",
            table: "Sagas",
            columns: new[] { "Id", "Type", "Tenant", },
            schema: "sagas");

        migrationBuilder.CreateIndex(
            name: "IX_Sagas_Tenant",
            table: "Sagas",
            column: "Tenant",
            schema: "sagas");
    }
}
