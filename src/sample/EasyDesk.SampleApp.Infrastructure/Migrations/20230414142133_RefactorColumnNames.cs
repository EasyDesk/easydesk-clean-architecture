using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenantId",
                schema: "domain",
                table: "Pets",
                newName: "Tenant");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_TenantId",
                schema: "domain",
                table: "Pets",
                newName: "IX_Pets_Tenant");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                schema: "domain",
                table: "People",
                newName: "Tenant");

            migrationBuilder.RenameIndex(
                name: "IX_People_TenantId",
                schema: "domain",
                table: "People",
                newName: "IX_People_Tenant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tenant",
                schema: "domain",
                table: "Pets",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_Tenant",
                schema: "domain",
                table: "Pets",
                newName: "IX_Pets_TenantId");

            migrationBuilder.RenameColumn(
                name: "Tenant",
                schema: "domain",
                table: "People",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_People_Tenant",
                schema: "domain",
                table: "People",
                newName: "IX_People_TenantId");
        }
    }
}
