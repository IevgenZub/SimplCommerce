using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class NoLandingDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LandingDate",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnLandingDate",
                table: "Catalog_Product");

            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "Catalog_Product",
                newName: "Terminal");

            migrationBuilder.AddColumn<int>(
                name: "DurationHours",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReturnDurationHours",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReturnDurationMinutes",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationHours",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnDurationHours",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnDurationMinutes",
                table: "Catalog_Product");

            migrationBuilder.RenameColumn(
                name: "Terminal",
                table: "Catalog_Product",
                newName: "Sku");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LandingDate",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnLandingDate",
                table: "Catalog_Product",
                nullable: true);
        }
    }
}
