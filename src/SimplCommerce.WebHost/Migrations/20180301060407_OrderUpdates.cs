using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class OrderUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChildPrice",
                table: "Orders_OrderItem",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantityBaby",
                table: "Orders_OrderItem",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityChild",
                table: "Orders_OrderItem",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Orders_OrderAddress",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Orders_OrderAddress",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgencyReservationNumber",
                table: "Orders_Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalNumber",
                table: "Orders_Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PnrNumber",
                table: "Orders_Order",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Core_User",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Core_Address",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Core_Address",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSellThenBuy",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLastMinute",
                table: "Catalog_Product",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildPrice",
                table: "Orders_OrderItem");

            migrationBuilder.DropColumn(
                name: "QuantityBaby",
                table: "Orders_OrderItem");

            migrationBuilder.DropColumn(
                name: "QuantityChild",
                table: "Orders_OrderItem");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Orders_OrderAddress");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Orders_OrderAddress");

            migrationBuilder.DropColumn(
                name: "AgencyReservationNumber",
                table: "Orders_Order");

            migrationBuilder.DropColumn(
                name: "ExternalNumber",
                table: "Orders_Order");

            migrationBuilder.DropColumn(
                name: "PnrNumber",
                table: "Orders_Order");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Core_User");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Core_Address");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Core_Address");

            migrationBuilder.DropColumn(
                name: "IsFirstSellThenBuy",
                table: "Catalog_Product");

            migrationBuilder.DropColumn(
                name: "IsLastMinute",
                table: "Catalog_Product");
        }
    }
}
