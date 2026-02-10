using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace KobiMuhendislikTicket.Hubs
{
    [Authorize(Roles = "Admin")]
    public class DashboardStatsHub : Hub
    {
        private readonly ILogger<DashboardStatsHub> _logger;
        private const string AdminGroup = "admin-dashboard";

        public DashboardStatsHub(ILogger<DashboardStatsHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Admin connected to DashboardStatsHub: {ConnectionId}", Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Admin disconnected from DashboardStatsHub: {ConnectionId}", Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
