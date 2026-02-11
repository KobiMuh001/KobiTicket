using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetByTenantIdAsync(int tenantId); 
        Task<Ticket?> GetByIdAsync(int id);
        Task<List<Ticket>> GetAllAsync();
        Task AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket); 
        Task AddCommentAsync(TicketComment comment);
        Task UpdateStatusDirectlyAsync(int id, TicketStatus newStatus);
        Task AddHistoryAsync(TicketHistory history);
        Task AssignToStaffAsync(int ticketId, string staffName);
        Task UpdatePriorityAsync(int ticketId, TicketPriority priority);
        Task UpdatePriorityDirectlyAsync(int id, TicketPriority newPriority);
        Task<List<Ticket>> GetFilteredTicketsAsync(int? tenantId, TicketStatus? status, TicketPriority? priority, string? assignedPerson);
        Task<int> GetTotalTenantsCountAsync();
        Task<int> GetTotalAssetsCountAsync();
        Task<List<TicketHistory>> GetHistoryAsync(int ticketId);
        Task<List<TicketComment>> GetCommentsAsync(int ticketId);
        Task<(List<Ticket> tickets, int totalCount)> GetAllTicketsPagedAsync(int pageNumber, int pageSize);
    }
}