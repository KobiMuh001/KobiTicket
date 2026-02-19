using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class DropLookupItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lookups");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Lookups",
                columns: new[] { "Id", "CreatedDate", "DisplayName", "IsDeleted", "Name", "SortOrder", "Type", "TypeId", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Teknik Destek", false, "Teknik Destek", 1, "Department", 1, null },
                    { 2, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Satış", false, "Satış", 2, "Department", 1, null },
                    { 3, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Muhasebe", false, "Muhasebe", 3, "Department", 1, null },
                    { 4, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Yönetim", false, "Yönetim", 4, "Department", 1, null },
                    { 5, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Diğer", false, "Diğer", 5, "Department", 1, null },
                    { 6, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Admin", false, "Admin", 1, "Role", 2, null },
                    { 7, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Çalışan", false, "Staff", 2, "Role", 2, null },
                    { 8, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Müşteri", false, "Customer", 3, "Role", 2, null },
                    { 9, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Düşük", false, "Low", 1, "Priority", 3, null },
                    { 10, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Orta", false, "Medium", 2, "Priority", 3, null },
                    { 11, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Yüksek", false, "High", 3, "Priority", 3, null },
                    { 12, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Kritik", false, "Critical", 4, "Priority", 3, null },
                    { 13, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Açık", false, "Open", 1, "Status", 4, null },
                    { 14, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "İşleniyor", false, "Processing", 2, "Status", 4, null },
                    { 15, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Müşteri Bekleniyor", false, "WaitingForCustomer", 3, "Status", 4, null },
                    { 16, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Çözüldü", false, "Resolved", 4, "Status", 4, null },
                    { 17, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Kapandı", false, "Closed", 5, "Status", 4, null }
                });

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
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1765));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1789));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1802));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1806));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1811));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1818));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1825));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1829));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1864));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1869));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1872));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1876));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1880));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1883));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1888));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1891));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 134,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1895));

            migrationBuilder.UpdateData(
                table: "SystemParameters",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedDate",
                value: new DateTime(2026, 2, 18, 16, 33, 9, 322, DateTimeKind.Unspecified).AddTicks(1898));

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_Type",
                table: "Lookups",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_TypeId",
                table: "Lookups",
                column: "TypeId");
        }
    }
}
