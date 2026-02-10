using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetByTenantIdAsync(Guid tenantId); 
        Task<Ticket?> GetByIdAsync(Guid id);
        Task<List<Ticket>> GetAllAsync();
        Task AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket); 
        Task AddCommentAsync(TicketComment comment);
        Task UpdateStatusDirectlyAsync(Guid id, TicketStatus newStatus);
        Task AddHistoryAsync(TicketHistory history);
        Task AssignToStaffAsync(Guid ticketId, string staffName);
        Task UpdatePriorityAsync(Guid ticketId, TicketPriority priority);
        Task UpdatePriorityDirectlyAsync(Guid id, TicketPriority newPriority);
        Task<List<Ticket>> GetFilteredTicketsAsync(Guid? tenantId, TicketStatus? status, TicketPriority? priority, string? assignedPerson);
        Task<int> GetTotalTenantsCountAsync();
        Task<int> GetTotalAssetsCountAsync();
        Task<List<TicketHistory>> GetHistoryAsync(Guid ticketId);
        Task<List<TicketComment>> GetCommentsAsync(Guid ticketId);
    }
}