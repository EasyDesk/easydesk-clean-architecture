using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.Sqlite;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "domain");

        migrationBuilder.CreateTable(
            name: "People",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                FirstName = table.Column<string>(type: "TEXT", nullable: false),
                LastName = table.Column<string>(type: "TEXT", nullable: false),
                DateOfBirth = table.Column<string>(type: "TEXT", nullable: false),
                Tenant = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                Residence_StreetType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Residence_StreetName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Residence_StreetNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Residence_City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Residence_District = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Residence_Province = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Residence_Region = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Residence_State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Residence_Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Approved = table.Column<bool>(type: "INTEGER", nullable: false),
                _Version = table.Column<long>(type: "INTEGER", nullable: false)
            },
            schema: "domain",
            constraints: table =>
            {
                table.PrimaryKey("PK_People", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Pets",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Nickname = table.Column<string>(type: "TEXT", nullable: false),
                PersonId = table.Column<Guid>(type: "TEXT", nullable: false),
                Tenant = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                _Version = table.Column<long>(type: "INTEGER", nullable: false)
            },
            schema: "domain",
            constraints: table =>
            {
                table.PrimaryKey("PK_Pets", x => x.Id);
                table.ForeignKey(
                    name: "FK_Pets_People_PersonId",
                    column: x => x.PersonId,
                    principalTable: "People",
                    principalColumn: "Id",
                    principalSchema: "domain",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_People_Tenant",
            table: "People",
            column: "Tenant",
            schema: "domain");

        migrationBuilder.CreateIndex(
            name: "IX_Pets_PersonId",
            table: "Pets",
            column: "PersonId",
            schema: "domain");

        migrationBuilder.CreateIndex(
            name: "IX_Pets_Tenant",
            table: "Pets",
            column: "Tenant",
            schema: "domain");
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
    }
}
