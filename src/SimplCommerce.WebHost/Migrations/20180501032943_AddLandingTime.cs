using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class AddLandingTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "Catalog_Product",
                newName: "Baggage");

            migrationBuilder.AddColumn<bool>(
                name: "IsNextDayLanding",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LandingTime",
                table: "Catalog_Product",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNextDayLanding",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "LandingTime",
                table: "Catalog_Product");

            migrationBuilder.RenameColumn(
                name: "Baggage",
                table: "Catalog_Product",
                newName: "DisplayOrder");
        }
    }
}
