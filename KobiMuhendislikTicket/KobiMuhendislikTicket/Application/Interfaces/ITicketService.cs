using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    
    public interface ITicketService
    {
        Task<Result<Ticket>> GetTicketByIdAsync(Guid id);
        Task<Result<List<Ticket>>> GetAllTicketsAsync();
        Task<Result<PaginatedTicketsDto>> GetAllTicketsPagedAsync(int pageNumber = 1, int pageSize = 20);
        Task<Result<List<Ticket>>> GetTenantTicketsAsync(Guid tenantId);
        Task<Result> CreateTicketAsync(Ticket ticket);
        Task<Result> UpdateTicketStatusAsync(Guid ticketId, int newStatus);
        Task<Result> UpdateTicketPriorityAsync(Guid ticketId, int newPriority);
        Task<Result> AssignTicketToPersonAsync(Guid ticketId, string personName);
        Task<Result> ResolveTicketAsync(Guid ticketId, string solutionNote, string resolvedBy);
        Task<Result<DashboardStatsDto>> GetAdminDashboardStatsAsync();
        Task<Result<List<Ticket>>> GetFilteredTicketsAsync(Guid? tenantId = null, TicketStatus? status = null, TicketPriority? priority = null, string? assignedPerson = null);
        Task<Result> AddCommentAsync(Guid ticketId, string message, string author, bool isAdmin, string source = "Customer");
        Task<Result> SaveTicketImageAsync(Guid ticketId, string imagePath);
    }
}
