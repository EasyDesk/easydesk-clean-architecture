using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.Sqlite.Migrations.Sagas;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "sagas");

        migrationBuilder.CreateTable(
            name: "Sagas",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                Type = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                Tenant = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                State = table.Column<string>(type: "TEXT", nullable: false),
                State_Old = table.Column<byte[]>(type: "BLOB", nullable: false),
                Version = table.Column<int>(type: "INTEGER", nullable: true)
            },
            schema: "sagas",
            constraints: table =>
            {
                table.PrimaryKey("PK_Sagas", x => new { x.Id, x.Type, x.Tenant });
            });

        migrationBuilder.CreateIndex(
            name: "IX_Sagas_Tenant",
            table: "Sagas",
            column: "Tenant",
            schema: "sagas");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Sagas",
            schema: "sagas");
    }
}
