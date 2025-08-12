using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auditing;

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

        migrationBuilder.AddPrimaryKey(
            name: "PK_AuditProperties",
            table: "AuditProperties",
            columns: ["AuditRecordId", "Key", "Value",],
            schema: "audit");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_AuditProperties",
            table: "AuditProperties",
            schema: "audit");

        migrationBuilder.AddPrimaryKey(
            name: "PK_AuditProperties",
            table: "AuditProperties",
            columns: ["AuditRecordId", "Key",],
            schema: "audit");
    }
}
