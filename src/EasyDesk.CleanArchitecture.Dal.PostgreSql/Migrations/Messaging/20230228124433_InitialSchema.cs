using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Messaging;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "messaging");

        migrationBuilder.CreateTable(
            name: "Inbox",
            schema: "messaging",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false)
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
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Content = table.Column<byte[]>(type: "bytea", nullable: false),
                Headers = table.Column<byte[]>(type: "bytea", nullable: false),
                DestinationAddress = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Outbox", x => x.Id);
            });
    }

    /// <inheritdoc />
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
