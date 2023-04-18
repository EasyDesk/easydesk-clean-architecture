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
            schema: "domain",
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
                Residence_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                Id = table.Column<int>(type: "int", nullable: false),
                Nickname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Tenant = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
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
            name: "IX_People_Tenant",
            schema: "domain",
            table: "People",
            column: "Tenant");

        migrationBuilder.CreateIndex(
            name: "IX_Pets_PersonId",
            schema: "domain",
            table: "Pets",
            column: "PersonId");

        migrationBuilder.CreateIndex(
            name: "IX_Pets_Tenant",
            schema: "domain",
            table: "Pets",
            column: "Tenant");
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
