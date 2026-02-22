using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddNumericKeyToSystemParameters2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumericKey",
                table: "SystemParameters",
                type: "int",
                nullable: true);

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
                columns: new[] { "CreatedDate", "NumericKey" },
                values: new object[] { new DateTime(2026, 2, 20, 8, 18, 10, 891, DateTimeKind.Unspecified).AddTicks(2991), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumericKey",
                table: "SystemParameters");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$7DIIIY4AlStfFiUdADXhc.S2f2GppLRmcnPgcq5.2/M60RepywS/i");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$LvTwlIAobuuwQiHTDdJT8O0h7fDcbOivjER0iMQyo//ZTaI14WwYq");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$dTLcDA5HlIwYKZfT3YoCUOaCUfKXGzrHCXhc6S5DwswL.ee86glJ2");

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(223));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(274));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(278));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(281));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(283));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(293));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(295));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(298));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(300));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(302));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(562));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(565));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(568));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(571));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(573));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(576));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(578));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(580));
        }
    }
}
