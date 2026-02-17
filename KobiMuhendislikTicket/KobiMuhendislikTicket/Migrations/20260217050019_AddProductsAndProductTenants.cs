using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddProductsAndProductTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTenants",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTenants", x => new { x.ProductId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_ProductTenants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTenants_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedDate", "Description", "IsDeleted", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Finans süreçleri için temel ürün modülü.", false, "izRP Finans Modülü", null },
                    { 2, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "İK operasyonları için temel ürün modülü.", false, "izRP İnsan Kaynakları", null },
                    { 3, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Muhasebe süreçleri için temel ürün modülü.", false, "izRP Muhasebe", null }
                });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$rPE2wMfnPGfCc2KQCeu/QO.PppVYVrvQEqt.5KZRsSuELTboj3jXW");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$KpIzpNpHOIaTazx7DYEaLeYl9SJWCEbzvJHl6IhVwi6MiFpMY1b.i");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$pr4SQNmlERiOLLJtNI1QreEuzDs24vGnBIgdD.5IeP2xUVT8A1/3m");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTenants_TenantId",
                table: "ProductTenants",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductTenants");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hikAL20jzs.cMkFXPhXtuOzZhKRGn25Zsev28bToiTLPdaJdNUiRi");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$4cSAKuXMwaOLvWgSEBG5L.o8PGd2Rq6ImmPYAeN8LFpv5GSY7Zvbu");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$7anXae85HwHLvPo1U6c8B.FAnhtrC0HT6bFttVIjJ1lUOUSereM82");
        }
    }
}
