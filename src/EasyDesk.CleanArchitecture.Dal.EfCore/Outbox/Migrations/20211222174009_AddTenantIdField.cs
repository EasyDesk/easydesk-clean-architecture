using Microsoft.EntityFrameworkCore.Migrations;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox.Migrations;

public partial class AddTenantIdField : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TenantId",
            schema: "outbox",
            table: "Messages",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TenantId",
            schema: "outbox",
            table: "Messages");
    }
}
