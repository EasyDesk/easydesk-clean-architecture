using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidenceToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "domain",
                table: "People",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<AddressModel>(
                name: "Residence",
                schema: "domain",
                table: "People",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Residence",
                schema: "domain",
                table: "People");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "domain",
                table: "People",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
