using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedStaffId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedStaffId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$H0EsXpavaPZcDY8lmnIW9um24QZAUmsTKPh3iIAGNgOE8wSCPyMne");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$AzgaWJXt9T/Ch7Yp91OKzO/csliUuhR2kQw5E8dzv6eHmt9.jwNhO");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$AVDMBLz7t54KV0xJk0U3V.VRsTAeLHo0i6CuTHrtfuBQeORbK0k1e");

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(863));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(901));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(904));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(906));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(908));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(913));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(916));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(918));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(920));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(921));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(939));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(941));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(943));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(945));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(947));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(949));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(950));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 22, 15, 34, 5, 28, DateTimeKind.Unspecified).AddTicks(952));

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedStaffId",
                table: "Tickets",
                column: "AssignedStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Staffs_AssignedStaffId",
                table: "Tickets",
                column: "AssignedStaffId",
                principalTable: "Staffs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Staffs_AssignedStaffId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AssignedStaffId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AssignedStaffId",
                table: "Tickets");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$NW8LYAVpzHO3HfTCG930/e7rV59BgT1vnW3uSS1Npdj4o9En78cRC");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Ji.2mHPQ2sIw7bbTAwpTF./TSfpmpgPS.TLXWCZPje9QAn2XXi1V.");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$t5.Y/OugRwehFTdmi4oN..9Dg06WHmAKMBZqrk5Tz0XzS7uqhY0LW");

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 752, DateTimeKind.Unspecified).AddTicks(9986));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(37));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(44));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(49));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(54));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(70));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(75));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(81));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(87));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(92));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(127));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(132));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(137));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(142));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(147));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(152));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(157));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(161));
        }
    }
}
