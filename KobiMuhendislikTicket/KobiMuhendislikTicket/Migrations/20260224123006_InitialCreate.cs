using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Group = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumericKey = table.Column<int>(type: "int", nullable: true),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxConcurrentTickets = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staffs_SystemParameters_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "SystemParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarrantyEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTenants",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    WarrantyEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTenants", x => new { x.ProductId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_ProductTenants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTenants_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    AssignedPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedStaffId = table.Column<int>(type: "int", nullable: true),
                    TicketCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tickets_Staffs_AssignedStaffId",
                        column: x => x.AssignedStaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: true),
                    TargetUserId = table.Column<int>(type: "int", nullable: true),
                    TargetTenantId = table.Column<int>(type: "int", nullable: true),
                    IsForAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TicketComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdminReply = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketComments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    ActionBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHistories_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketImages_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SystemParameters",
                columns: new[] { "Id", "CreatedDate", "DataType", "Description", "Group", "IsActive", "IsDeleted", "Key", "NumericKey", "SortOrder", "UpdatedDate", "Value", "Value2" },
                values: new object[,]
                {
                    { 100, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(482), "String", "Düşük öncelik", "TicketPriority", true, false, "Düşük", 1, 1, null, "Low", "#6C757D" },
                    { 101, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(562), "String", "Orta öncelik", "TicketPriority", true, false, "Orta", 2, 2, null, "Medium", "#17A2B8" },
                    { 102, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(565), "String", "Yüksek öncelik", "TicketPriority", true, false, "Yüksek", 3, 3, null, "High", "#FD7E14" },
                    { 103, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(567), "String", "Kritik öncelik", "TicketPriority", true, false, "Kritik", 4, 4, null, "Critical", "#DC3545" },
                    { 110, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(568), "String", "Açık", "TicketStatus", true, false, "Açık", 1, 1, null, "Open", "#007BFF" },
                    { 111, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(570), "String", "İşleniyor", "TicketStatus", true, false, "İşleniyor", 2, 2, null, "Processing", "#FFC107" },
                    { 112, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(572), "String", "Müşteri Bekleniyor", "TicketStatus", true, false, "Müşteri Bekleniyor", 3, 3, null, "WaitingForCustomer", "#6F42C1" },
                    { 113, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(573), "String", "Çözüldü", "TicketStatus", true, false, "Çözüldü", 4, 4, null, "Resolved", "#28A745" },
                    { 114, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(575), "String", "Kapandı", "TicketStatus", true, false, "Kapandı", 5, 5, null, "Closed", "#343A40" },
                    { 120, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(576), "String", "Admin role", "UserRole", true, false, "Admin", 1, 1, null, "Admin", null },
                    { 121, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(578), "String", "Staff role", "UserRole", true, false, "Staff", 2, 2, null, "Staff", null },
                    { 122, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(579), "String", "Customer role", "UserRole", true, false, "Customer", 3, 3, null, "Customer", null },
                    { 130, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(581), "String", "Teknik Destek", "Department", true, false, "TeknikDestek", 1, 1, null, "Teknik Destek", null },
                    { 131, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(582), "String", "Satış", "Department", true, false, "Satis", 2, 2, null, "Satış", null },
                    { 132, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(583), "String", "Muhasebe", "Department", true, false, "Muhasebe", 3, 3, null, "Muhasebe", null },
                    { 133, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(585), "String", "Yönetim", "Department", true, false, "Yonetim", 4, 4, null, "Yönetim", null },
                    { 134, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(586), "String", "Diğer", "Department", true, false, "Diger", 5, 5, null, "Diğer", null },
                    { 200, new DateTime(2026, 2, 24, 15, 30, 6, 297, DateTimeKind.Unspecified).AddTicks(587), "Int", "Varsayılan ticket limiti", "General", true, false, "DefaultTicketLimit", 1, 1, null, "15", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_TenantId",
                table: "Assets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TicketId",
                table: "Notifications",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTenants_TenantId",
                table: "ProductTenants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Email",
                table: "Staffs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameters_Group",
                table: "SystemParameters",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Email",
                table: "Tenants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TaxNumber",
                table: "Tenants",
                column: "TaxNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Username",
                table: "Tenants",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId",
                table: "TicketComments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistories_TicketId",
                table: "TicketHistories",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketImages_TicketId",
                table: "TicketImages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedStaffId",
                table: "Tickets",
                column: "AssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProductId",
                table: "Tickets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TenantId",
                table: "Tickets",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProductTenants");

            migrationBuilder.DropTable(
                name: "TicketComments");

            migrationBuilder.DropTable(
                name: "TicketHistories");

            migrationBuilder.DropTable(
                name: "TicketImages");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "SystemParameters");
        }
    }
}
