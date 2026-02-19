using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Staffs");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Staffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketPriorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketPriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CreatedDate", "DisplayName", "IsDeleted", "Name", "SortOrder", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6888), "Teknik Destek", false, "Teknik Destek", 1, null },
                    { 2, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6891), "Satış", false, "Satış", 2, null },
                    { 3, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6894), "Muhasebe", false, "Muhasebe", 3, null },
                    { 4, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6896), "Yönetim", false, "Yönetim", 4, null },
                    { 5, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6901), "Diğer", false, "Diğer", 5, null }
                });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DepartmentId", "PasswordHash" },
                values: new object[] { 1, "$2a$11$dkxTaUU7IkvbVLVnPkeZZ.x6NHChmijsQl9ReFsMs9uSIXPxZZLbi" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DepartmentId", "PasswordHash" },
                values: new object[] { 1, "$2a$11$WDK2Ig93HNwBZZLp34fUceRes2O4pk3RXKDUTz0eUsDBM37nEqKsG" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DepartmentId", "PasswordHash" },
                values: new object[] { 2, "$2a$11$lZkjcDh268K2LcL787ER7eVyegvcVvK/ZSh4cYOsw0eBYRBQaKvN2" });

            migrationBuilder.InsertData(
                table: "TicketPriorities",
                columns: new[] { "Id", "CreatedDate", "DisplayName", "IsDeleted", "Name", "SortOrder", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6514), "Düşük", false, "Low", 1, null },
                    { 2, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6518), "Orta", false, "Medium", 2, null },
                    { 3, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6521), "Yüksek", false, "High", 3, null },
                    { 4, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6524), "Kritik", false, "Critical", 4, null }
                });

            migrationBuilder.InsertData(
                table: "TicketStatuses",
                columns: new[] { "Id", "CreatedDate", "DisplayName", "IsDeleted", "Name", "SortOrder", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6733), "Açık", false, "Open", 1, null },
                    { 2, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6736), "İşleniyor", false, "Processing", 2, null },
                    { 3, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6739), "Müşteri Bekleniyor", false, "WaitingForCustomer", 3, null },
                    { 4, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6741), "Çözüldü", false, "Resolved", 4, null },
                    { 5, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6744), "Kapandı", false, "Closed", 5, null }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "CreatedDate", "DisplayName", "IsDeleted", "Name", "SortOrder", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6811), "Admin", false, "Admin", 1, null },
                    { 2, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6814), "Çalışan", false, "Staff", 2, null },
                    { 3, new DateTime(2026, 2, 18, 11, 0, 10, 742, DateTimeKind.Utc).AddTicks(6817), "Müşteri", false, "Customer", 3, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Departments_DepartmentId",
                table: "Staffs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Departments_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "TicketPriorities");

            migrationBuilder.DropTable(
                name: "TicketStatuses");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Staffs");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Department", "PasswordHash" },
                values: new object[] { "Teknik Destek", "$2a$11$cTwQ.jQm7My6yGwI7wWpU.by5.7kEE9yozJm/zOcSNvq8Y7cbWP1W" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Department", "PasswordHash" },
                values: new object[] { "Teknik Destek", "$2a$11$YPRdP.qPm9sZ8az/q3HuauBrdMAM6EG2w8PKHwwcpHJE.xFrCG16W" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Department", "PasswordHash" },
                values: new object[] { "Satış", "$2a$11$YDnRiNKcvQQYjM8otdH6CudJstUIIHvaOtDypEFpl4KZ9GE6sY7gC" });
        }
    }
}
