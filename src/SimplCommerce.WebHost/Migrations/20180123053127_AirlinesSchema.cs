using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class AirlinesSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CountryId",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone1",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone2",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone3",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone4",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendEmails",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorClass",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorType",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Core_Vendor",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminBlackList",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminIsLastMinute",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminIsSpecialOffer",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminNotifyAgencies",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminNotifyLastPassanger",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdminPasExpirityRule",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminPayLater",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminPayLaterRule",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminReturnBlackList",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminReturnIsLastMinute",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminReturnIsSpecialOffer",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminReturnNotifyAgencies",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminReturnNotifyLastPassanger",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdminReturnPasExpirityRule",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminReturnPayLater",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminReturnPayLaterRule",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AdminRoundTrip",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AdminRoundTripOperatorId",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DepartureDate",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightClass",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRoundTrip",
                table: "Catalog_Product",
                nullable: false,
                defaultValue:false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LandingDate",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservationNumber",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReturnAircraftId",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReturnCarrierId",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnDepartureDate",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnFlightNumber",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReturnLandingDate",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnTerminal",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnVia",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SaleRtOnly",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoldSeats",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Via",
                table: "Catalog_Product",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Product_ReturnAircraftId",
                table: "Catalog_Product",
                column: "ReturnAircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Product_ReturnCarrierId",
                table: "Catalog_Product",
                column: "ReturnCarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalog_Product_VendorId",
                table: "Catalog_Product",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Catalog_Product_Tax_TaxClass_ReturnAircraftId",
                table: "Catalog_Product",
                column: "ReturnAircraftId",
                principalTable: "Tax_TaxClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Catalog_Product_Catalog_Brand_ReturnCarrierId",
                table: "Catalog_Product",
                column: "ReturnCarrierId",
                principalTable: "Catalog_Brand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Catalog_Product_Core_Vendor_VendorId",
                table: "Catalog_Product",
                column: "VendorId",
                principalTable: "Core_Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catalog_Product_Tax_TaxClass_ReturnAircraftId",
                table: "Catalog_Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Catalog_Product_Catalog_Brand_ReturnCarrierId",
                table: "Catalog_Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Catalog_Product_Core_Vendor_VendorId",
                table: "Catalog_Product");

            migrationBuilder.DropIndex(
                name: "IX_Catalog_Product_ReturnAircraftId",
                table: "Catalog_Product");

            migrationBuilder.DropIndex(
                name: "IX_Catalog_Product_ReturnCarrierId",
                table: "Catalog_Product");

            migrationBuilder.DropIndex(
                name: "IX_Catalog_Product_VendorId",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Phone1",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Phone2",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Phone3",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Phone4",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "SendEmails",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "VendorClass",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "VendorType",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Core_Vendor");

            migrationBuilder.DropColumn(
                name: "AdminBlackList",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminIsLastMinute",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminIsSpecialOffer",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminNotifyAgencies",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminNotifyLastPassanger",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminPasExpirityRule",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminPayLater",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminPayLaterRule",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnBlackList",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnIsLastMinute",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnIsSpecialOffer",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnNotifyAgencies",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnNotifyLastPassanger",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnPasExpirityRule",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnPayLater",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminReturnPayLaterRule",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminRoundTrip",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "AdminRoundTripOperatorId",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "FlightClass",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "IsRoundTrip",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "LandingDate",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReservationNumber",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnAircraftId",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnCarrierId",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnDepartureDate",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnFlightNumber",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnLandingDate",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnTerminal",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "ReturnVia",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "SaleRtOnly",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "SoldSeats",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "Via",
                table: "Catalog_Product");
        }
    }
}
