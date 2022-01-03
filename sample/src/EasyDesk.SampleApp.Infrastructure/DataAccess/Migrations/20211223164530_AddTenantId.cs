using Microsoft.EntityFrameworkCore.Migrations;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Migrations;

public partial class AddTenantId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TenantId",
            schema: "entities",
            table: "People",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TenantId",
            schema: "entities",
            table: "People");
    }
}
