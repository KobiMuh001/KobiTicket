using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Assets");
        }
    }
}
