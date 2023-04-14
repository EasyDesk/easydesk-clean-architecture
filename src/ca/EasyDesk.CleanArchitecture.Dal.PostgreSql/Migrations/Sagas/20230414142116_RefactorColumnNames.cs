using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Sagas
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
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
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
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048);
        }
    }
}
