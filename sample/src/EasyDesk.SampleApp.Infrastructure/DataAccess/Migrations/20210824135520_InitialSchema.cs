using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Migrations;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "entities");

        migrationBuilder.CreateTable(
            name: "People",
            schema: "entities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Married = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_People", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "People",
            schema: "entities");
    }
}
