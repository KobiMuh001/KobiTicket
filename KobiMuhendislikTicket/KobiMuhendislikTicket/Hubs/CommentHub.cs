using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace KobiMuhendislikTicket.Hubs
{
    [Authorize]
    public class CommentHub : Hub
    {
        private readonly ILogger<CommentHub> _logger;

        public CommentHub(ILogger<CommentHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
            _logger.LogInformation("User {UserId} joined ticket group: {TicketId}", Context.UserIdentifier, ticketId);
        }

        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
            _logger.LogInformation("User {UserId} left ticket group: {TicketId}", Context.UserIdentifier, ticketId);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
