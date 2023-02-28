using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Sagas;

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
            schema: "sagas",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Type = table.Column<string>(type: "text", nullable: false),
                TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                State = table.Column<byte[]>(type: "bytea", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sagas", x => new { x.Id, x.Type, x.TenantId });
            });

        migrationBuilder.CreateIndex(
            name: "IX_Sagas_TenantId",
            schema: "sagas",
            table: "Sagas",
            column: "TenantId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Sagas",
            schema: "sagas");
    }
}
