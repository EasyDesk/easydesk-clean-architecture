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
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Type = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                Tenant = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                State = table.Column<byte[]>(type: "bytea", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: true),
            },
            schema: "sagas",
            constraints: table =>
            {
                table.PrimaryKey("PK_Sagas", x => new { x.Id, x.Type, x.Tenant, });
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
