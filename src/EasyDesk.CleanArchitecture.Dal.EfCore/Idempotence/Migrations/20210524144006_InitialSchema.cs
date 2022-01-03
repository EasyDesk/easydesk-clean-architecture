using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence.Migrations;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "idempotence");

        migrationBuilder.CreateTable(
            name: "HandledEvents",
            schema: "idempotence",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_HandledEvents", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "HandledEvents",
            schema: "idempotence");
    }
}
