using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox.Migrations;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "outbox");

        migrationBuilder.CreateTable(
            name: "Messages",
            schema: "outbox",
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
                table.PrimaryKey("PK_Messages", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Messages",
            schema: "outbox");
    }
}
