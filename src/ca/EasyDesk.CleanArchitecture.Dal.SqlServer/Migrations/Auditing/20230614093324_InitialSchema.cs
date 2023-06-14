using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Auditing;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "audit");

        migrationBuilder.CreateTable(
            name: "AuditRecords",
            schema: "audit",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Type = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Identity = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                Success = table.Column<bool>(type: "bit", nullable: false),
                Instant = table.Column<DateTime>(type: "datetime2", nullable: false),
                Tenant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditRecords", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AuditIdentityAttributeModel",
            schema: "audit",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AuditRecordId = table.Column<long>(type: "bigint", nullable: false),
                Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditIdentityAttributeModel", x => x.Id);
                table.ForeignKey(
                    name: "FK_AuditIdentityAttributeModel_AuditRecords_AuditRecordId",
                    column: x => x.AuditRecordId,
                    principalSchema: "audit",
                    principalTable: "AuditRecords",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AuditRecordPropertyModel",
            schema: "audit",
            columns: table => new
            {
                AuditRecordId = table.Column<long>(type: "bigint", nullable: false),
                Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditRecordPropertyModel", x => new { x.AuditRecordId, x.Key });
                table.ForeignKey(
                    name: "FK_AuditRecordPropertyModel_AuditRecords_AuditRecordId",
                    column: x => x.AuditRecordId,
                    principalSchema: "audit",
                    principalTable: "AuditRecords",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditIdentityAttributeModel_AuditRecordId",
            schema: "audit",
            table: "AuditIdentityAttributeModel",
            column: "AuditRecordId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_Instant",
            schema: "audit",
            table: "AuditRecords",
            column: "Instant",
            descending: Array.Empty<bool>());

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_Tenant",
            schema: "audit",
            table: "AuditRecords",
            column: "Tenant");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuditIdentityAttributeModel",
            schema: "audit");

        migrationBuilder.DropTable(
            name: "AuditRecordPropertyModel",
            schema: "audit");

        migrationBuilder.DropTable(
            name: "AuditRecords",
            schema: "audit");
    }
}
