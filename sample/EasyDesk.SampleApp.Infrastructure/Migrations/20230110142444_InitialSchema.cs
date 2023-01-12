using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "domain");

        migrationBuilder.CreateSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "domain",
            incrementBy: 10);

        migrationBuilder.CreateTable(
            name: "People",
            schema: "domain",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "text", nullable: false),
                LastName = table.Column<string>(type: "text", nullable: false),
                DateOfBirth = table.Column<LocalDate>(type: "date", nullable: false),
                TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_People", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Pets",
            schema: "domain",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false),
                Nickname = table.Column<string>(type: "text", nullable: false),
                PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pets", x => x.Id);
                table.ForeignKey(
                    name: "FK_Pets_People_PersonId",
                    column: x => x.PersonId,
                    principalSchema: "domain",
                    principalTable: "People",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_People_TenantId",
            schema: "domain",
            table: "People",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "IX_Pets_PersonId",
            schema: "domain",
            table: "Pets",
            column: "PersonId");

        migrationBuilder.CreateIndex(
            name: "IX_Pets_TenantId",
            schema: "domain",
            table: "Pets",
            column: "TenantId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Pets",
            schema: "domain");

        migrationBuilder.DropTable(
            name: "People",
            schema: "domain");

        migrationBuilder.DropSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "domain");
    }
}
