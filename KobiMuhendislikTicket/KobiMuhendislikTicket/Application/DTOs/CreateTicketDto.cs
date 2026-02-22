using KobiMuhendislikTicket.Domain.Enums;

namespace KobiMuhendislikTicket.Application.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string? TicketCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public string? AssignedPerson { get; set; }
        public int? AssignedStaffId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int TenantId { get; set; }
        public string? CompanyName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ImagePath { get; set; }
    }

    public class CreateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = 2; 
        public int? ProductId { get; set; }
        public string? ImagePath { get; set; }
    }

    public class UpdateTicketStatusDto
    {
        public int NewStatus { get; set; }
    }

    public class UpdateTicketPriorityDto
    {
        public int NewPriority { get; set; }
    }

    public class AssignTicketDto
    {
        public string PersonName { get; set; } = string.Empty;
    }

    public class ResolveTicketDto
    {
        public string SolutionNote { get; set; } = string.Empty;
        public string ResolvedBy { get; set; } = string.Empty;
    }

    public class AddCommentDto
    {
        public string Message { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }

    public class CustomerCommentDto
    {
        public string Message { get; set; } = string.Empty;
    }
}