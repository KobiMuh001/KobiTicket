using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class lookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Lookups",
                columns: new[] { "Id", "CreatedDate", "DisplayName", "IsDeleted", "Name", "SortOrder", "Type", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Teknik Destek", false, "Teknik Destek", 1, "Department", null },
                    { 2, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Satış", false, "Satış", 2, "Department", null },
                    { 3, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Muhasebe", false, "Muhasebe", 3, "Department", null },
                    { 4, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Yönetim", false, "Yönetim", 4, "Department", null },
                    { 5, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Diğer", false, "Diğer", 5, "Department", null },
                    { 6, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Admin", false, "Admin", 1, "Role", null },
                    { 7, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Çalışan", false, "Staff", 2, "Role", null },
                    { 8, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Müşteri", false, "Customer", 3, "Role", null },
                    { 9, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Düşük", false, "Low", 1, "Priority", null },
                    { 10, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Orta", false, "Medium", 2, "Priority", null },
                    { 11, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Yüksek", false, "High", 3, "Priority", null },
                    { 12, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Kritik", false, "Critical", 4, "Priority", null },
                    { 13, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Açık", false, "Open", 1, "Status", null },
                    { 14, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "İşleniyor", false, "Processing", 2, "Status", null },
                    { 15, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Müşteri Bekleniyor", false, "WaitingForCustomer", 3, "Status", null },
                    { 16, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Çözüldü", false, "Resolved", 4, "Status", null },
                    { 17, new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "Kapandı", false, "Closed", 5, "Status", null }
                });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$Y8gaQt/j/iAI8y0.osLesed8vv84YJqH7RFEAxhxTbsJnAVjYdZ3m" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$wolcvp4MS/aaZ/Q2bGOBWOFG7Bvs413yh1.8qI4C5X9LquoHJqq/u" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$7Rz.bw9G4gf.6GGoHxk/7O/oKiBn5MnSSN5uNwaN6UgaaYY4asGJC" });

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Lookups_DepartmentId",
                table: "Staffs",
                column: "DepartmentId",
                principalTable: "Lookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Lookups_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropTable(
                name: "Lookups");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$dkxTaUU7IkvbVLVnPkeZZ.x6NHChmijsQl9ReFsMs9uSIXPxZZLbi" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$WDK2Ig93HNwBZZLp34fUceRes2O4pk3RXKDUTz0eUsDBM37nEqKsG" });

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$lZkjcDh268K2LcL787ER7eVyegvcVvK/ZSh4cYOsw0eBYRBQaKvN2" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Departments_DepartmentId",
                table: "Staffs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
