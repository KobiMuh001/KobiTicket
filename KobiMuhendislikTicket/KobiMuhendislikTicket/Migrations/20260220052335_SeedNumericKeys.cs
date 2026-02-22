using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class SeedNumericKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 752, DateTimeKind.Unspecified).AddTicks(9986), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(37), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(44), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(49), 4 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(54), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(70), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(75), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(81), 4 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(87), 5 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(92), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(127), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(132), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(137), 1 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(142), 2 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(147), 3 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(152), 4 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(157), 5 });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 23, 33, 753, DateTimeKind.Unspecified).AddTicks(161));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$co4jf6A8iuhBGdex/5.JCeZ0gHBE7GKWHX6pErHOxoFsxcaZTqyTi");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$OZdO9PiUSSHa20sXjHkPMeyMhBeIKJskGQqmgFWwMkvKMEWG8C44K");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$IT6/WVM8fBF8Kw8dCsW1suNFfKqorcUbd04kHjmh/rrp3cVVyos4K");

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2900), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2935), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2938), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2940), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2942), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2951), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2953), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2956), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2958), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2960), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2975), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2977), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2979), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2982), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2984), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2986), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2988), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2991));
        }
    }
}
