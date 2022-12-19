using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddPets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Pets",
            schema: "domain",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Nickname = table.Column<string>(type: "text", nullable: false),
                PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<string>(type: "text", nullable: true)
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
    }
}
