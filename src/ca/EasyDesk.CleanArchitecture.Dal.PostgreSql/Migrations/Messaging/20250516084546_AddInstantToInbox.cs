using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Messaging;

/// <inheritdoc />
public partial class AddInstantToInbox : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Instant>(
            name: "Instant",
            schema: "messaging",
            table: "Inbox",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Instant",
            schema: "messaging",
            table: "Inbox");
    }
}
