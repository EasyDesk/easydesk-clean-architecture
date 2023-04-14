using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Auditing
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
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "User",
                schema: "audit",
                table: "AuditRecords",
                type: "nvarchar(1024)",
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
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "audit",
                table: "AuditRecords",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
