using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    public partial class AddNumericKeyToSystemParameters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // add nullable NumericKey column
            migrationBuilder.AddColumn<int>(
                name: "NumericKey",
                table: "SystemParameters",
                type: "int",
                nullable: true);

            // Attempt to cast existing string keys to integers where possible
            migrationBuilder.Sql(@"UPDATE SystemParameters SET NumericKey = TRY_CAST([Key] AS int) WHERE NumericKey IS NULL");

            // Backfill known mappings for TicketStatus (adjust as needed)
            migrationBuilder.Sql(@"
                UPDATE SystemParameters SET NumericKey = 1 WHERE [Group] = 'TicketStatus' AND [Key] = 'Open';
                UPDATE SystemParameters SET NumericKey = 2 WHERE [Group] = 'TicketStatus' AND [Key] = 'InProgress';
                UPDATE SystemParameters SET NumericKey = 3 WHERE [Group] = 'TicketStatus' AND [Key] = 'Resolved';
                UPDATE SystemParameters SET NumericKey = 4 WHERE [Group] = 'TicketStatus' AND [Key] = 'Closed';
            ");

            // Backfill known mappings for TicketPriority (adjust as needed)
            migrationBuilder.Sql(@"
                UPDATE SystemParameters SET NumericKey = 1 WHERE [Group] = 'TicketPriority' AND [Key] = 'Low';
                UPDATE SystemParameters SET NumericKey = 2 WHERE [Group] = 'TicketPriority' AND [Key] = 'Medium';
                UPDATE SystemParameters SET NumericKey = 3 WHERE [Group] = 'TicketPriority' AND [Key] = 'High';
            ");

            // For any remaining rows, optionally set NumericKey = Id to ensure uniqueness
            migrationBuilder.Sql(@"UPDATE SystemParameters SET NumericKey = Id WHERE NumericKey IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumericKey",
                table: "SystemParameters");
        }
    }
}
