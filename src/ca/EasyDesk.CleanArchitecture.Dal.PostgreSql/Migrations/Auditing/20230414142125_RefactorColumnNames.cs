using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auditing
{
    /// <inheritdoc />
    public partial class RefactorColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenantId",
                schema: "audit",
                table: "AuditRecords",
                newName: "Tenant");

            migrationBuilder.RenameIndex(
                name: "IX_AuditRecords_TenantId",
                schema: "audit",
                table: "AuditRecords",
                newName: "IX_AuditRecords_Tenant");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "audit",
                table: "AuditRecords",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tenant",
                schema: "audit",
                table: "AuditRecords",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditRecords_Tenant",
                schema: "audit",
                table: "AuditRecords",
                newName: "IX_AuditRecords_TenantId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "audit",
                table: "AuditRecords",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048);
        }
    }
}
