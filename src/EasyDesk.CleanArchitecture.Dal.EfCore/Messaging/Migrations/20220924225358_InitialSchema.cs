using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging.Migrations;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "messaging");

        migrationBuilder.CreateTable(
            name: "Inbox",
            schema: "messaging",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Inbox", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Outbox",
            schema: "messaging",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                Headers = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                DestinationAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Outbox", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Inbox",
            schema: "messaging");

        migrationBuilder.DropTable(
            name: "Outbox",
            schema: "messaging");
    }
}
