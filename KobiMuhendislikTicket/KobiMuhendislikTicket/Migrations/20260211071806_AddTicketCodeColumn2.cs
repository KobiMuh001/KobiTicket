using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCodeColumn2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ri..v503qVGhylO06aF4duP0n6oMRrvZ0gnOog68epSHInV4syVrS");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$LuSLkOBkVxPPFFC2kO8WMOAlHQiV9xmhge91PXirOg/9ElCrQfAyW");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$QIsWAO7g2AxhTX4emwZI2eeEaiSJnXlRxPR4C/DXT7NgxvhF8Pkhq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
