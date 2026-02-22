using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    
    public interface ITicketService
    {
        Task<Result<Ticket>> GetTicketByIdAsync(int id);
        Task<Result<List<Ticket>>> GetAllTicketsAsync();
        Task<Result<PaginatedTicketsDto>> GetAllTicketsPagedAsync(int pageNumber = 1, int pageSize = 20);
        Task<Result<List<Ticket>>> GetTenantTicketsAsync(int tenantId);
        Task<Result> CreateTicketAsync(Ticket ticket);
        Task<Result> UpdateTicketStatusAsync(int ticketId, int newStatus);
        Task<Result> UpdateTicketPriorityAsync(int ticketId, int newPriority);
        Task<Result> AssignTicketToPersonAsync(int ticketId, string personName);
        Task<Result> ResolveTicketAsync(int ticketId, string solutionNote, string resolvedBy);
        Task<Result<DashboardStatsDto>> GetAdminDashboardStatsAsync();
        Task<Result<List<Ticket>>> GetFilteredTicketsAsync(int? tenantId = null, TicketStatus? status = null, TicketPriority? priority = null, string? assignedPerson = null);
        Task<Result> AddCommentAsync(int ticketId, string message, string author, bool isAdmin, string source = "Customer");
        Task<Result> SaveTicketImageAsync(int ticketId, string imagePath);
        Task BroadcastTicketUpdateAsync(int ticketId);
        Task BroadcastDashboardStatsAsync();
    }
}
