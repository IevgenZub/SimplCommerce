using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class RenameBasic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "Catalog_Product",
                newName: "Destination");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Catalog_Product",
                newName: "Departure");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Destination",
                table: "Catalog_Product",
                newName: "ShortDescription");

            migrationBuilder.RenameColumn(
                name: "Departure",
                table: "Catalog_Product",
                newName: "Description");
        }
    }
}
