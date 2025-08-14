using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.Sqlite.Migrations.Auditing;

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
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Type = table.Column<int>(type: "INTEGER", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                Description = table.Column<string>(type: "TEXT", nullable: true),
                Success = table.Column<bool>(type: "INTEGER", nullable: false),
                Instant = table.Column<string>(type: "TEXT", nullable: false),
                Tenant = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
            },
            schema: "audit",
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditRecords", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AuditIdentities",
            columns: table => new
            {
                AuditRecordId = table.Column<long>(type: "INTEGER", nullable: false),
                IdentityRealm = table.Column<string>(type: "TEXT", nullable: false),
                Identity = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
            },
            schema: "audit",
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditIdentities", x => new { x.AuditRecordId, x.IdentityRealm, });
                table.ForeignKey(
                    name: "FK_AuditIdentities_AuditRecords_AuditRecordId",
                    column: x => x.AuditRecordId,
                    principalTable: "AuditRecords",
                    principalColumn: "Id",
                    principalSchema: "audit",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AuditProperties",
            columns: table => new
            {
                AuditRecordId = table.Column<long>(type: "INTEGER", nullable: false),
                Key = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: false),
            },
            schema: "audit",
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditProperties", x => new { x.AuditRecordId, x.Key, x.Value, });
                table.ForeignKey(
                    name: "FK_AuditProperties_AuditRecords_AuditRecordId",
                    column: x => x.AuditRecordId,
                    principalTable: "AuditRecords",
                    principalColumn: "Id",
                    principalSchema: "audit",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AuditIdentityAttributes",
            columns: table => new
            {
                AuditRecordId = table.Column<long>(type: "INTEGER", nullable: false),
                Realm = table.Column<string>(type: "TEXT", nullable: false),
                Key = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: false),
            },
            schema: "audit",
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditIdentityAttributes", x => new { x.AuditRecordId, x.Realm, x.Key, x.Value, });
                table.ForeignKey(
                    name: "FK_AuditIdentityAttributes_AuditIdentities_AuditRecordId_Realm",
                    columns: x => new { x.AuditRecordId, x.Realm, },
                    principalTable: "AuditIdentities",
                    principalColumns: new[] { "AuditRecordId", "IdentityRealm", },
                    principalSchema: "audit",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_Instant",
            table: "AuditRecords",
            column: "Instant",
            schema: "audit",
            descending: []);

        migrationBuilder.CreateIndex(
            name: "IX_AuditRecords_Tenant",
            table: "AuditRecords",
            column: "Tenant",
            schema: "audit");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuditIdentityAttributes",
            schema: "audit");

        migrationBuilder.DropTable(
            name: "AuditProperties",
            schema: "audit");

        migrationBuilder.DropTable(
            name: "AuditIdentities",
            schema: "audit");

        migrationBuilder.DropTable(
            name: "AuditRecords",
            schema: "audit");
    }
}
