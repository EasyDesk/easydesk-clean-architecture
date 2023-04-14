using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Sagas
{
    /// <inheritdoc />
    public partial class RefactorColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenantId",
                schema: "sagas",
                table: "Sagas",
                newName: "Tenant");

            migrationBuilder.RenameIndex(
                name: "IX_Sagas_TenantId",
                schema: "sagas",
                table: "Sagas",
                newName: "IX_Sagas_Tenant");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                schema: "sagas",
                table: "Sagas",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tenant",
                schema: "sagas",
                table: "Sagas",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Sagas_Tenant",
                schema: "sagas",
                table: "Sagas",
                newName: "IX_Sagas_TenantId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                schema: "sagas",
                table: "Sagas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);
        }
    }
}
