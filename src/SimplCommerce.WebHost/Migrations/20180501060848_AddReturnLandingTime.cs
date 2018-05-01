using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class AddReturnLandingTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReturnIsNextDayLanding",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnLandingTime",
                table: "Catalog_Product",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnIsNextDayLanding",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnLandingTime",
                table: "Catalog_Product");
        }
    }
}
