namespace KobiMuhendislikTicket.Application.DTOs
{
    

    public class StaffDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int MaxConcurrentTickets { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateStaffDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Department { get; set; } = "Teknik Destek";
        public int MaxConcurrentTickets { get; set; } = 10;
    }

    public class UpdateStaffDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public bool? IsActive { get; set; }
        public int? MaxConcurrentTickets { get; set; }
        public string? NewPassword { get; set; }
    }

    public class StaffLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ResetStaffPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public class StaffWorkloadDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int MaxConcurrentTickets { get; set; }
        
        
        public int AssignedTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ProcessingTickets { get; set; }
        public int ResolvedToday { get; set; }
        public int ResolvedThisWeek { get; set; }
        public bool IsAvailable { get; set; } 
        public double WorkloadPercentage { get; set; } 
    }

    public class AssignTicketToStaffDto
    {
        public int StaffId { get; set; }
        public string? Note { get; set; } 
    }

    public class BulkAssignDto
    {
        public List<int> TicketIds { get; set; } = new();
        public int StaffId { get; set; }
    }

    public class AutoAssignResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public int StaffId { get; set; }
    }
}
