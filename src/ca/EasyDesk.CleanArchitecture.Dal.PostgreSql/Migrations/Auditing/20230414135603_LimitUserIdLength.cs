using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Auditing
{
    /// <inheritdoc />
    public partial class LimitUserIdLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "audit",
                table: "AuditRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "audit",
                table: "AuditRecords",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "User",
                schema: "audit",
                table: "AuditRecords",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User",
                schema: "audit",
                table: "AuditRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "audit",
                table: "AuditRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "audit",
                table: "AuditRecords",
                type: "text",
                nullable: true);
        }
    }
}
