using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class AirportsRus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartureRus",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationRus",
                table: "Catalog_Product",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureRus",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "DestinationRus",
                table: "Catalog_Product");
        }
    }
}
