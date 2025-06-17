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
            table: "AuditProperties",
            schema: "audit");

        migrationBuilder.AlterColumn<string>(
            name: "Value",
            table: "AuditProperties",
            type: "nvarchar(450)",
            schema: "audit",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AuditProperties",
            table: "AuditProperties",
            columns: ["AuditRecordId", "Key", "Value"],
            schema: "audit");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_AuditProperties",
            table: "AuditProperties",
            schema: "audit");

        migrationBuilder.AlterColumn<string>(
            name: "Value",
            table: "AuditProperties",
            type: "nvarchar(max)",
            schema: "audit",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AuditProperties",
            table: "AuditProperties",
            columns: ["AuditRecordId", "Key"],
            schema: "audit");
    }
}
