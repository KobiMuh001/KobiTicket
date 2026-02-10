using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class SeedStaffData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Staffs",
                columns: new[] { "Id", "CreatedDate", "Department", "Email", "FullName", "IsActive", "IsDeleted", "MaxConcurrentTickets", "Phone", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 2, 5, 8, 57, 37, 88, DateTimeKind.Utc).AddTicks(9576), "Teknik Destek", "ahmet.yilmaz@kobi.com", "Ahmet Yılmaz", true, false, 10, "(532) 111 2233", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 2, 5, 8, 57, 37, 88, DateTimeKind.Utc).AddTicks(9587), "Teknik Destek", "mehmet.kaya@kobi.com", "Mehmet Kaya", true, false, 8, "(533) 222 3344", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 2, 5, 8, 57, 37, 88, DateTimeKind.Utc).AddTicks(9591), "Satış", "ayse.demir@kobi.com", "Ayşe Demir", true, false, 5, "(534) 333 4455", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));
        }
    }
}
