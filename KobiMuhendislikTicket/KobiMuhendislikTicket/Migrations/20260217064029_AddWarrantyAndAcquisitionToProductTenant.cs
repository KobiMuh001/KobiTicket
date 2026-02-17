using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddWarrantyAndAcquisitionToProductTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcquisitionDate",
                table: "ProductTenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyEndDate",
                table: "ProductTenants",
                type: "datetime2",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcquisitionDate",
                table: "ProductTenants");

            migrationBuilder.DropColumn(
                name: "WarrantyEndDate",
                table: "ProductTenants");

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
        }
    }
}
