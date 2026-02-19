namespace KobiMuhendislikTicket.Application.DTOs
{
    

    public class AdminDashboardDto
    {
        
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ProcessingTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int TotalAssets { get; set; }
        public int TotalTenants { get; set; }
        public int CriticalTicketCount { get; set; }
        public int TodayNewTickets { get; set; }
        public int ThisWeekResolvedTickets { get; set; }
        public double AverageResolutionTimeHours { get; set; }

        
        public List<TicketsByStatusDto> TicketsByStatus { get; set; } = new();
        public List<TicketsByPriorityDto> TicketsByPriority { get; set; } = new();
        public List<DailyTicketStatsDto> Last7DaysStats { get; set; } = new();

        
        public List<AssetTicketCountDto> TopFailingAssets { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<StaffPerformanceDto> StaffPerformance { get; set; } = new();
        public List<TicketListItemDto> UrgentTickets { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionBy { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
    }

    public class TicketsByStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TicketsByPriorityDto
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class StaffPerformanceDto
    {
        public string StaffName { get; set; } = string.Empty;
        public int AssignedTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int OpenTickets { get; set; }
        public double AverageResolutionHours { get; set; }
    }

    public class DailyTicketStatsDto
    {
        public DateTime Date { get; set; }
        public int Created { get; set; }
        public int Resolved { get; set; }
    }

 

    public class TenantListItemDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int AssetCount { get; set; }
        public int TotalTickets { get; set; }
        public int OpenTicketCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TenantDetailDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string TaxNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedDate { get; set; }

        
        public int TotalAssets { get; set; }
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }

        
        public List<TenantAssetDto> Assets { get; set; } = new();
        public List<TicketListItemDto> RecentTickets { get; set; } = new();
    }

    public class TenantAssetDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? WarrantyEndDate { get; set; }
        public bool IsUnderWarranty { get; set; }
        public int TicketCount { get; set; }
    }

    

    public class AssetListItemDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? WarrantyEndDate { get; set; }
        public bool IsUnderWarranty { get; set; }
        public int TicketCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AssetDetailDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? WarrantyEndDate { get; set; }
        public bool IsUnderWarranty { get; set; }
        public int DaysUntilWarrantyExpires { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;

        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }

        
        public List<TicketListItemDto> TicketHistory { get; set; } = new();
    }

   

    public class TicketListItemDto
    {
        public int Id { get; set; }
        public string? TicketCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Priority { get; set; }
        public string? AssignedPerson { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string? ProductName { get; set; }
        public int? ProductId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int CommentCount { get; set; }
        public bool IsOverdue { get; set; } 
    }

    public class TicketDetailDto
    {
        public int Id { get; set; }
        public string? TicketCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Priority { get; set; }
        public string? AssignedPerson { get; set; }
        public string? ImagePath { get; set; }
        public List<string> ImagePaths { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
        public string? TenantPhone { get; set; }

        
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }

        public List<TicketCommentDto> Comments { get; set; } = new();
        public List<TicketHistoryItemDto> History { get; set; } = new();
    }

    public class TicketCommentDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public bool IsAdminReply { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TicketHistoryItemDto
    {
        public string Description { get; set; } = string.Empty;
        public string ActionBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class TicketFilterDto
    {
        public int? TenantId { get; set; }
        public int? Status { get; set; }
        public int? Priority { get; set; }
        public string? AssignedPerson { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    

    public class PerformanceReportDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        
        public int TotalTicketsCreated { get; set; }
        public int TotalTicketsResolved { get; set; }
        public double AverageResolutionTimeHours { get; set; }
        public double SlaComplianceRate { get; set; }

        
        public List<DailyTicketStatsDto> DailyStats { get; set; } = new();

        
        public List<StaffPerformanceDto> StaffPerformance { get; set; } = new();
    }

    public class WarrantyReportDto
    {
        public int TotalAssets { get; set; }
        public int UnderWarranty { get; set; }
        public int ExpiredWarranty { get; set; }
        public int ExpiringSoon { get; set; } 
        public List<AssetWarrantyAlertDto> ExpiringAssets { get; set; } = new();
    }

    public class AssetWarrantyAlertDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public DateTime WarrantyEndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class TenantReportDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; } 
        public List<TenantActivityDto> TopActiveTenants { get; set; } = new();
        public List<TenantActivityDto> InactiveTenants { get; set; } = new();
    }

    public class TenantActivityDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int TicketCount { get; set; }
        public DateTime? LastTicketDate { get; set; }
    }

    public class PaginatedTicketsDto
    {
        public List<TicketListItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
