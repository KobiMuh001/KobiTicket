using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$paHE2QpiAo6f.dj1qEI2busB8imS.0rIPGRQKT82zewAqHEYeWqqy");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "PasswordHash",
                value: "$2a$11$jxvoD4vQpT8nyqteHubrN.9arLWrtHeY3PIwGiRWUoBJlE7LSoRLu");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$20sHmj7Vpdyy8OBjeNBPrebKuk5YEqzcsuPKCMxkk3Eyg0WWeMK6i");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Staffs");
        }
    }
}
