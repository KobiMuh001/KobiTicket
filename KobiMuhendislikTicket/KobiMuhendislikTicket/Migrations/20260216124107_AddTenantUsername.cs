using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Tenants",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hikAL20jzs.cMkFXPhXtuOzZhKRGn25Zsev28bToiTLPdaJdNUiRi");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$4cSAKuXMwaOLvWgSEBG5L.o8PGd2Rq6ImmPYAeN8LFpv5GSY7Zvbu");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$7anXae85HwHLvPo1U6c8B.FAnhtrC0HT6bFttVIjJ1lUOUSereM82");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Username",
                table: "Tenants",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Username",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Tenants");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$V5rBun0XiT3gFmba2U0OKuAmTm89plbLTgtI2eZ8U9Dqg9RZiimqy");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Q3A1kkeGDbIpLM72FksNcOg0ZPi.YBhAZBk7fjvF66ZNVkWrE8SHe");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$C00n3i7vnyWaG6OEOp7DcunFQRoHCzfAvudAjLnrk4LXPRBcfEpca");
        }
    }
}
