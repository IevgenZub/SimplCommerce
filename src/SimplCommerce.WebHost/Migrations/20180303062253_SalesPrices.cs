using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class SalesPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AgencyChildPrice",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AgencyPrice",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ChildPrice",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PassengerChildPrice",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PassengerPrice",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyChildPrice",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AgencyPrice",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ChildPrice",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "PassengerChildPrice",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "PassengerPrice",
                table: "Catalog_Product");
        }
    }
}
