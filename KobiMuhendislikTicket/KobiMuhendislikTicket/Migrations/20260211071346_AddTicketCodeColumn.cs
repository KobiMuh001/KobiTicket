using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCodeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketCode",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$nRAtgPGRVN4hAXN4/CE/a.IDSK6LwHFhsyDPz780WDc7riKe/hJ6m");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$/fSUtgNRS.jt6b/quw4JYOsWeTDa8uvg0jrt.zPwuGnNlT5J25AJ2");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$OoKgUk2miB4Q6p6ewEt9Y.U0JlUTGgzUyPOAa36SgiXRjkhIxxlIG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketCode",
                table: "Tickets");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$2WaF2y7lyz2Zs0IJQOJGWekeg2/SSh3wUS5A9LKKWIeWZjBAsP.J6");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$gaO49XkZQKlKc7x0.jDPaejwScpQCWkg7DGV8hcSSu1dlHCmicVZC");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$FT521Ik/j7RqYQeKpNw5gu/HV0c6pwhcG/kvzv0yvOZXuWtVDK3ka");
        }
    }
}
