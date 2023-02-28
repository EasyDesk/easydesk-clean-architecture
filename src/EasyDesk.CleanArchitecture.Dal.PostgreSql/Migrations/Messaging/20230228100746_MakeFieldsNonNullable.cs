using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Messaging;

/// <inheritdoc />
public partial class MakeFieldsNonNullable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<byte[]>(
            name: "Headers",
            schema: "messaging",
            table: "Outbox",
            type: "bytea",
            nullable: false,
            defaultValue: Array.Empty<byte>(),
            oldClrType: typeof(byte[]),
            oldType: "bytea",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<byte[]>(
            name: "Headers",
            schema: "messaging",
            table: "Outbox",
            type: "bytea",
            nullable: true,
            oldClrType: typeof(byte[]),
            oldType: "bytea");
    }
}
