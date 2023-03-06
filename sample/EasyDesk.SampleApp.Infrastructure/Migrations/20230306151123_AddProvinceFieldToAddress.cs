using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyDesk.SampleApp.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddProvinceFieldToAddress : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Residence",
            schema: "domain",
            table: "People");

        migrationBuilder.AddColumn<string>(
            name: "Residence_City",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Residence_Country",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Residence_District",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Residence_Province",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Residence_Region",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Residence_State",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Residence_StreetName",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "Residence_StreetNumber",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Residence_StreetType",
            schema: "domain",
            table: "People",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Residence_City",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_Country",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_District",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_Province",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_Region",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_State",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_StreetName",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_StreetNumber",
            schema: "domain",
            table: "People");

        migrationBuilder.DropColumn(
            name: "Residence_StreetType",
            schema: "domain",
            table: "People");

        migrationBuilder.AddColumn<AddressModel>(
            name: "Residence",
            schema: "domain",
            table: "People",
            type: "jsonb",
            nullable: false);
    }
}
