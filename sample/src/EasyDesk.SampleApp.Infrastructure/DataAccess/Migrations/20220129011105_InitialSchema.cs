using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Migrations;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "domain");

        migrationBuilder.CreateTable(
            name: "People",
            schema: "domain",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Married = table.Column<bool>(type: "bit", nullable: false),
                TenantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_People", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_People_TenantId",
            schema: "domain",
            table: "People",
            column: "TenantId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "People",
            schema: "domain");
    }
}
