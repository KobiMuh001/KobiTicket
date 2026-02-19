using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Lookups_DepartmentId",
                table: "Staffs");

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Group = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                });

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

            migrationBuilder.InsertData(
                table: "SystemParameters",
                columns: new[] { "Id", "CreatedDate", "DataType", "Description", "Group", "IsActive", "IsDeleted", "Key", "UpdatedDate", "Value" },
                values: new object[,]
                {
                    { 100, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3655), "String", "Düşük öncelik", "TicketPriority", true, false, "Low", null, "Low" },
                    { 101, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3669), "String", "Orta öncelik", "TicketPriority", true, false, "Medium", null, "Medium" },
                    { 102, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3678), "String", "Yüksek öncelik", "TicketPriority", true, false, "High", null, "High" },
                    { 103, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3680), "String", "Kritik öncelik", "TicketPriority", true, false, "Critical", null, "Critical" },
                    { 110, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3682), "String", "Açık", "TicketStatus", true, false, "Open", null, "Open" },
                    { 111, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3692), "String", "İşleniyor", "TicketStatus", true, false, "Processing", null, "Processing" },
                    { 112, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3697), "String", "Müşteri Bekleniyor", "TicketStatus", true, false, "WaitingForCustomer", null, "WaitingForCustomer" },
                    { 113, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3700), "String", "Çözüldü", "TicketStatus", true, false, "Resolved", null, "Resolved" },
                    { 114, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3713), "String", "Kapandı", "TicketStatus", true, false, "Closed", null, "Closed" },
                    { 120, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3715), "String", "Admin role", "UserRole", true, false, "Admin", null, "Admin" },
                    { 121, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3718), "String", "Staff role", "UserRole", true, false, "Staff", null, "Staff" },
                    { 122, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3720), "String", "Customer role", "UserRole", true, false, "Customer", null, "Customer" },
                    { 130, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3722), "String", "Teknik Destek", "Department", true, false, "TeknikDestek", null, "Teknik Destek" },
                    { 131, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3724), "String", "Satış", "Department", true, false, "Satis", null, "Satış" },
                    { 132, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3726), "String", "Muhasebe", "Department", true, false, "Muhasebe", null, "Muhasebe" },
                    { 133, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3728), "String", "Yönetim", "Department", true, false, "Yonetim", null, "Yönetim" },
                    { 134, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3730), "String", "Diğer", "Department", true, false, "Diger", null, "Diğer" },
                    { 200, new DateTime(2026, 2, 18, 15, 0, 43, 4, DateTimeKind.Unspecified).AddTicks(3732), "Int", "Varsayılan ticket limiti", "General", true, false, "DefaultTicketLimit", null, "15" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameters_Group",
                table: "SystemParameters",
                column: "Group");

            // Map existing Staff.DepartmentId values (which referenced old Lookup ids)
            // to the new SystemParameters ids for Department group.
            // Old Lookup ids: 1..5 => New SystemParameter ids: 130..134
            migrationBuilder.Sql("UPDATE [Staffs] SET [DepartmentId] = 130 WHERE [DepartmentId] = 1;");
            migrationBuilder.Sql("UPDATE [Staffs] SET [DepartmentId] = 131 WHERE [DepartmentId] = 2;");
            migrationBuilder.Sql("UPDATE [Staffs] SET [DepartmentId] = 132 WHERE [DepartmentId] = 3;");
            migrationBuilder.Sql("UPDATE [Staffs] SET [DepartmentId] = 133 WHERE [DepartmentId] = 4;");
            migrationBuilder.Sql("UPDATE [Staffs] SET [DepartmentId] = 134 WHERE [DepartmentId] = 5;");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_SystemParameters_DepartmentId",
                table: "Staffs",
                column: "DepartmentId",
                principalTable: "SystemParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_SystemParameters_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$QyHvS3q0yR9HVn8w5YvEHuXd07rE4GYl2wF0n67MA5.C9mHTRIrdm");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$SVqRSDgDtMMU9yoxUQBvf.aj6FZmPdVC6A5Jz8AFA7.bPDzw3ahcK");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$uUj/ryjuq6JXq9Azmc861OFWookwdvA9.0LgmIZsDchLnJVu0fp7y");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Lookups_DepartmentId",
                table: "Staffs",
                column: "DepartmentId",
                principalTable: "Lookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
