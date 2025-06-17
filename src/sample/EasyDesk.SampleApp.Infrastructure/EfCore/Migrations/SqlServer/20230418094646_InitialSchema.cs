using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Migrations.SqlServer;

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
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                Tenant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Residence_StreetType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Residence_StreetName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Residence_StreetNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Residence_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Residence_District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Residence_Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Residence_Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Residence_State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Residence_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                Id = table.Column<int>(type: "int", nullable: false),
                Nickname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Tenant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
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

        migrationBuilder.DropSequence(
            name: "EntityFrameworkHiLoSequence",
            schema: "domain");
    }
}
