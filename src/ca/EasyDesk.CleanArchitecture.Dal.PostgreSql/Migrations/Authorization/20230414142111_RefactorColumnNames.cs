using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.Migrations.Authorization
{
    /// <inheritdoc />
    public partial class RefactorColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Tenants_TenantId",
                schema: "auth",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Tenants_TenantIdFk",
                schema: "auth",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                schema: "auth",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "auth",
                table: "UserRoles");

            migrationBuilder.RenameColumn(
                name: "TenantIdFk",
                schema: "auth",
                table: "UserRoles",
                newName: "TenantFk");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                schema: "auth",
                table: "UserRoles",
                newName: "Tenant");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                schema: "auth",
                table: "UserRoles",
                newName: "Role");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_TenantIdFk",
                schema: "auth",
                table: "UserRoles",
                newName: "IX_UserRoles_TenantFk");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_TenantId",
                schema: "auth",
                table: "UserRoles",
                newName: "IX_UserRoles_Tenant");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                schema: "auth",
                table: "RolePermissions",
                newName: "Tenant");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_TenantId",
                schema: "auth",
                table: "RolePermissions",
                newName: "IX_RolePermissions_Tenant");

            migrationBuilder.AddColumn<string>(
                name: "User",
                schema: "auth",
                table: "UserRoles",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                schema: "auth",
                table: "UserRoles",
                columns: new[] { "User", "Role", "Tenant" });

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Tenants_Tenant",
                schema: "auth",
                table: "RolePermissions",
                column: "Tenant",
                principalSchema: "auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Tenants_TenantFk",
                schema: "auth",
                table: "UserRoles",
                column: "TenantFk",
                principalSchema: "auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Tenants_Tenant",
                schema: "auth",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Tenants_TenantFk",
                schema: "auth",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                schema: "auth",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "User",
                schema: "auth",
                table: "UserRoles");

            migrationBuilder.RenameColumn(
                name: "TenantFk",
                schema: "auth",
                table: "UserRoles",
                newName: "TenantIdFk");

            migrationBuilder.RenameColumn(
                name: "Tenant",
                schema: "auth",
                table: "UserRoles",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "Role",
                schema: "auth",
                table: "UserRoles",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_TenantFk",
                schema: "auth",
                table: "UserRoles",
                newName: "IX_UserRoles_TenantIdFk");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_Tenant",
                schema: "auth",
                table: "UserRoles",
                newName: "IX_UserRoles_TenantId");

            migrationBuilder.RenameColumn(
                name: "Tenant",
                schema: "auth",
                table: "RolePermissions",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_Tenant",
                schema: "auth",
                table: "RolePermissions",
                newName: "IX_RolePermissions_TenantId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                schema: "auth",
                table: "UserRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                schema: "auth",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "TenantId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Tenants_TenantId",
                schema: "auth",
                table: "RolePermissions",
                column: "TenantId",
                principalSchema: "auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Tenants_TenantIdFk",
                schema: "auth",
                table: "UserRoles",
                column: "TenantIdFk",
                principalSchema: "auth",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
