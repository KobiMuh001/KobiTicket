using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class sortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "SystemParameters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$rhgJGqKimbK6DMB20V/0BOoJC.Io4ywIX80uRv2bG80qrwmDDJ2r2");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$heGAVf7pRGm/G86wE6G6ROSDCxOcHDytOWrZUn3GFA6W7YNvGJDrS");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$v1ECOGxsNygTGGPYKVoHpuiYFY2EBijaLXOTH33eTZ8raRYqByMPa");

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1765), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1789), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1802), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1806), 4 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1811), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1818), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1825), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1829), 4 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1864), 5 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1869), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1872), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1876), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1880), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1883), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1888), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1891), 4 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1895), 5 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "CreatedDate", "SortOrder" },
                values: new object[] { new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1898), 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "SystemParameters");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$IPLs1jyBcMR/T9G0I9hnEOrc9AaenzhKr.3lrHcrhB8IeGsgE1apG");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$xKYUNyEeHJpgU8PZgkVrKu7HN3jvE9A1g3ITZnTke9vjEOjlV0/Ea");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$IQM20Xk7umx2EIfqPksoPeEeuWwatEPcPOqaEnGuz.INquLLHrsgS");

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3655));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3669));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3678));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3680));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3682));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3692));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3697));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3700));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3713));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3715));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3718));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3720));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3722));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3724));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3726));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3728));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3730));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3732));
        }
    }
}
