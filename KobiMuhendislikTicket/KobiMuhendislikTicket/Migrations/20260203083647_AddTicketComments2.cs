using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketComments2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedPerson",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedPerson",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Tickets");
        }
    }
}
