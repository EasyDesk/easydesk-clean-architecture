using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Auditing;

/// <inheritdoc />
public partial class MultipleValuesForProperties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_AuditProperties",
            schema: "audit",
            table: "AuditProperties");

        migrationBuilder.AlterColumn<string>(
            name: "Value",
            schema: "audit",
            table: "AuditProperties",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AuditProperties",
            schema: "audit",
            table: "AuditProperties",
            columns: new[] { "AuditRecordId", "Key", "Value" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_AuditProperties",
            schema: "audit",
            table: "AuditProperties");

        migrationBuilder.AlterColumn<string>(
            name: "Value",
            schema: "audit",
            table: "AuditProperties",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AuditProperties",
            schema: "audit",
            table: "AuditProperties",
            columns: new[] { "AuditRecordId", "Key" });
    }
}
