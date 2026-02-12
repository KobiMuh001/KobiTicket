using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerNotificationTargetTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetTenantId",
                table: "Notifications",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetTenantId",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$sfPMBUHw7wJLorErzU5jmuWeO89zGTjFB0ENM6qsT0wldlAlvf3l.");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$KUcikctL36Q/h1ssZSwlOO6e2owIao2EYfhhbGsgG5I44mgkEr3fC");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$lMJ1oP3q1vAJS0XNP7kQXeRBJQCTapv7RCYX0qO5HBXW0EKhtr7Q6");
        }
    }
}
