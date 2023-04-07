using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.Migrations.Auditing;

/// <inheritdoc />
public partial class AddProperties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuditRecordPropertyModel",
            schema: "audit");
    }
}
