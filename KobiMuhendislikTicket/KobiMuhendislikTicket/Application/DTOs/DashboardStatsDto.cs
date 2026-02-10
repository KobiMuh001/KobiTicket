namespace KobiMuhendislikTicket.Application.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ProcessingTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int TotalAssets { get; set; }
        public int TotalTenants { get; set; }
        public int CriticalTicketCount { get; set; }

        public List<AssetTicketCountDto> TopFailingAssets { get; set; } = new();
    }

    public class AssetTicketCountDto
    {
        public required string ProductName { get; set; }
        public int TicketCount { get; set; }
    }
}