using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantLogoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Tenants");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$.AU2AvtoBtYHSKsSUCPVW.3v9BcZMdna2pcfqdR.46O20VARMKdk.");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$2df5xxuuJpo247TD/hbXre2MyunpO8CVq1QpqOKsb4/YSXNdaWN0a");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$A.53.nRSGuKrBxduvQYOK.nskKlGSfGo6rcjT5SIVAXJan7N/tqN6");
        }
    }
}
