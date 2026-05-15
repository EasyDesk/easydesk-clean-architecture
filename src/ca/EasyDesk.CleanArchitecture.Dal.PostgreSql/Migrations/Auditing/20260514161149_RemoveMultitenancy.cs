using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auditing;

/// <inheritdoc />
public partial class RemoveMultitenancy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AuditRecords_Tenant",
            table: "AuditRecords",
            schema: "audit");

        migrationBuilder.DropColumn(
            name: "Tenant",
            table: "AuditRecords",
            schema: "audit");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Tenant",
            table: "AuditRecords",
            type: "character varying(256)",
            maxLength: 256,
            schema: "audit",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_Tenant",
            table: "AuditRecords",
            column: "Tenant",
            schema: "audit");
    }
}
