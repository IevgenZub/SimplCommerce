using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SimplCommerce.WebHost.Migrations
{
    public partial class RegistrationInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders_OrderRegistrationAddress",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressId = table.Column<long>(nullable: false),
                    OrderId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders_OrderRegistrationAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_OrderRegistrationAddress_Orders_OrderAddress_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Orders_OrderAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_OrderRegistrationAddress_Orders_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders_Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderRegistrationAddress_AddressId",
                table: "Orders_OrderRegistrationAddress",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderRegistrationAddress_OrderId",
                table: "Orders_OrderRegistrationAddress",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders_OrderRegistrationAddress");
        }
    }
}
