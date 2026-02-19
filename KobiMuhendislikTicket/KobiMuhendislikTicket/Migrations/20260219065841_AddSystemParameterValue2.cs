using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemParameterValue2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Value2",
                table: "SystemParameters",
                type: "nvarchar(max)",
                nullable: true);

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
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(223), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(274), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(278), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(281), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(283), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(293), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(295), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(298), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(300), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(302), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(562), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(565), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(568), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(571), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(573), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(576), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(578), null });

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "CreatedDate", "Value2" },
                values: new object[] { new DateTime(2026, 2, 19, 9, 58, 40, 327, DateTimeKind.Unspecified).AddTicks(580), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value2",
                table: "SystemParameters");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$D/02LFiszj.NtgYmwio0Pub067WYuel8McCebt3a0xPQfyx2ND3Xe");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Xe.4z3qxXBQMz/uNCW32TerKlb.B39lc86pHrcgRlECKCC7myzmiG");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$RDLzBRi6pwy3VCb2Z6jffew0IdvOP788i.uQpPD8kMgTMT9IoW6.m");

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5538));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5577));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5580));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5583));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5585));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5602));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5604));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5606));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5609));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5611));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5630));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5633));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5635));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5638));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5640));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5642));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5644));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 19, 9, 49, 23, 512, DateTimeKind.Unspecified).AddTicks(5647));
        }
    }
}
