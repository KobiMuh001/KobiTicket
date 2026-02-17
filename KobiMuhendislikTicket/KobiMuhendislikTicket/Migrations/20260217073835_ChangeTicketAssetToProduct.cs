using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTicketAssetToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Assets_AssetId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "Tickets",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssetId",
                table: "Tickets",
                newName: "IX_Tickets_ProductId");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$cTwQ.jQm7My6yGwI7wWpU.by5.7kEE9yozJm/zOcSNvq8Y7cbWP1W");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$YPRdP.qPm9sZ8az/q3HuauBrdMAM6EG2w8PKHwwcpHJE.xFrCG16W");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$YDnRiNKcvQQYjM8otdH6CudJstUIIHvaOtDypEFpl4KZ9GE6sY7gC");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Products_ProductId",
                table: "Tickets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Products_ProductId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Tickets",
                newName: "AssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_ProductId",
                table: "Tickets",
                newName: "IX_Tickets_AssetId");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RUS4vuySFQ.jXOVh5U05dunEU4TBA.0O31YA7wuYKIu4h690pB272");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$7WprSPah1aVjckiQMVbtluIDd8fM6oI/TYQcombRHUdCs2TRJnf8u");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$S1Wg5nmJNHHVig80AvvgLuE9T/QPm70wXLEOp4EG4UqeSkS4xUtv.");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Assets_AssetId",
                table: "Tickets",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id");
        }
    }
}
