using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KobiMuhendislikTicket.Application.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==================== DASHBOARD ====================

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var today = DateTimeHelper.GetLocalNow().Date;
            var weekAgo = today.AddDays(-7);

            var tickets = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Asset)
                .ToListAsync();

            var histories = await _context.TicketHistories
                .OrderByDescending(h => h.CreatedDate)
                .Take(10)
                .ToListAsync();

            var totalTickets = tickets.Count;

            var dashboard = new AdminDashboardDto
            {
                TotalTickets = totalTickets,
                OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
                ProcessingTickets = tickets.Count(t => t.Status == TicketStatus.Processing),
                ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                CriticalTicketCount = tickets.Count(t => t.Priority == TicketPriority.Critical && t.Status != TicketStatus.Resolved),
                TodayNewTickets = tickets.Count(t => t.CreatedDate.Date == today),
                ThisWeekResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved && t.UpdatedDate >= weekAgo),
                TotalAssets = await _context.Assets.CountAsync(),
                TotalTenants = await _context.Tenants.CountAsync(),

                
                TicketsByStatus = Enum.GetValues<TicketStatus>()
                    .Select(s => new TicketsByStatusDto
                    {
                        Status = GetStatusName(s),
                        Count = tickets.Count(t => t.Status == s),
                        Percentage = totalTickets > 0 ? Math.Round(tickets.Count(t => t.Status == s) * 100.0 / totalTickets, 1) : 0
                    }).ToList(),

                
                TicketsByPriority = Enum.GetValues<TicketPriority>()
                    .Select(p => new TicketsByPriorityDto
                    {
                        Priority = GetPriorityName(p),
                        Count = tickets.Count(t => t.Priority == p),
                        Percentage = totalTickets > 0 ? Math.Round(tickets.Count(t => t.Priority == p) * 100.0 / totalTickets, 1) : 0
                    }).ToList(),

               
                Last7DaysStats = Enumerable.Range(0, 7)
                    .Select(i => today.AddDays(-i))
                    .Select(date => new DailyTicketStatsDto
                    {
                        Date = date,
                        Created = tickets.Count(t => t.CreatedDate.Date == date),
                        Resolved = tickets.Count(t => t.Status == TicketStatus.Resolved && t.UpdatedDate?.Date == date)
                    })
                    .OrderBy(d => d.Date)
                    .ToList(),

                
                TopFailingAssets = tickets
                    .Where(t => t.Asset != null)
                    .GroupBy(t => new { t.AssetId, t.Asset!.ProductName })
                    .Select(g => new AssetTicketCountDto
                    {
                        ProductName = g.Key.ProductName,
                        TicketCount = g.Count()
                    })
                    .OrderByDescending(x => x.TicketCount)
                    .Take(5)
                    .ToList(),

                
                RecentActivities = histories.Select(h => new RecentActivityDto
                {
                    TicketId = h.TicketId,
                    TicketTitle = tickets.FirstOrDefault(t => t.Id == h.TicketId)?.Title ?? "Bilinmeyen",
                    Action = h.Description,
                    ActionBy = h.ActionBy,
                    ActionDate = h.CreatedDate
                }).ToList(),

                
                StaffPerformance = tickets
                    .Where(t => !string.IsNullOrEmpty(t.AssignedPerson))
                    .GroupBy(t => t.AssignedPerson!)
                    .Select(g => new StaffPerformanceDto
                    {
                        StaffName = g.Key,
                        AssignedTickets = g.Count(),
                        ResolvedTickets = g.Count(t => t.Status == TicketStatus.Resolved),
                        OpenTickets = g.Count(t => t.Status != TicketStatus.Resolved)
                    })
                    .OrderByDescending(x => x.ResolvedTickets)
                    .ToList(),

                
                UrgentTickets = tickets
                    .Where(t => t.Status != TicketStatus.Resolved && (t.Priority == TicketPriority.Critical || t.Priority == TicketPriority.High))
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.CreatedDate)
                    .Take(5)
                    .Select(t => MapToTicketListItem(t))
                    .ToList()
            };

            return dashboard;
        }

        // ==================== TENANT ====================

        public async Task<PagedResultDto<TenantListItemDto>> GetTenantsAsync(int page = 1, int pageSize = 20, string? search = null)
        {
            var query = _context.Tenants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.CompanyName.Contains(search) || t.TaxNumber.Contains(search) || t.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var tenants = await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TenantListItemDto
                {
                    Id = t.Id,
                    CompanyName = t.CompanyName,
                    TaxNumber = t.TaxNumber,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    AssetCount = _context.Assets.Count(a => a.TenantId == t.Id),
                    TotalTickets = _context.Tickets.Count(ti => ti.TenantId == t.Id),
                    OpenTicketCount = _context.Tickets.Count(ti => ti.TenantId == t.Id && ti.Status != TicketStatus.Resolved),
                    CreatedDate = t.CreatedDate
                })
                .ToListAsync();

            return new PagedResultDto<TenantListItemDto>
            {
                Items = tenants,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<TenantDetailDto?> GetTenantDetailAsync(int tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return null;

            var assets = await _context.Assets
                .Where(a => a.TenantId == tenantId)
                .ToListAsync();

            var tickets = await _context.Tickets
                .Include(t => t.Asset)
                .Include(t => t.Tenant)
                .Where(t => t.TenantId == tenantId)
                .OrderByDescending(t => t.CreatedDate)
                .Take(10)
                .ToListAsync();

            return new TenantDetailDto
            {
                Id = tenant.Id,
                CompanyName = tenant.CompanyName,
                TaxNumber = tenant.TaxNumber,
                Email = tenant.Email,
                PhoneNumber = tenant.PhoneNumber,
                Address = null, 
                CreatedDate = tenant.CreatedDate,
                TotalAssets = assets.Count,
                TotalTickets = await _context.Tickets.CountAsync(t => t.TenantId == tenantId),
                OpenTickets = await _context.Tickets.CountAsync(t => t.TenantId == tenantId && t.Status != TicketStatus.Resolved),
                ResolvedTickets = await _context.Tickets.CountAsync(t => t.TenantId == tenantId && t.Status == TicketStatus.Resolved),
                Assets = assets.Select(a => new TenantAssetDto
                {
                    Id = a.Id,
                    ProductName = a.ProductName,
                    SerialNumber = a.SerialNumber,
                    Status = a.Status,
                    WarrantyEndDate = a.WarrantyEndDate,
                    IsUnderWarranty = a.WarrantyEndDate > DateTimeHelper.GetLocalNow(),
                    TicketCount = _context.Tickets.Count(t => t.AssetId == a.Id)
                }).ToList(),
                RecentTickets = tickets.Select(t => MapToTicketListItem(t)).ToList()
            };
        }

        // ==================== ASSET  ====================

        public async Task<PagedResultDto<AssetListItemDto>> GetAssetsAsync(int page = 1, int pageSize = 20, string? search = null, bool? underWarranty = null)
        {
            var query = _context.Assets.Include(a => a.Tenant).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.ProductName.Contains(search) || a.SerialNumber.Contains(search));
            }

            if (underWarranty.HasValue)
            {
                if (underWarranty.Value)
                    query = query.Where(a => a.WarrantyEndDate > DateTimeHelper.GetLocalNow());
                else
                    query = query.Where(a => a.WarrantyEndDate <= DateTimeHelper.GetLocalNow());
            }

            var totalCount = await query.CountAsync();

            var assets = await query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssetListItemDto
                {
                    Id = a.Id,
                    ProductName = a.ProductName,
                    SerialNumber = a.SerialNumber,
                    TenantName = a.Tenant != null ? a.Tenant.CompanyName : "Bilinmeyen",
                    TenantId = a.TenantId,
                    Status = a.Status,
                    WarrantyEndDate = a.WarrantyEndDate,
                    IsUnderWarranty = a.WarrantyEndDate > DateTimeHelper.GetLocalNow(),
                    TicketCount = _context.Tickets.Count(t => t.AssetId == a.Id)
                })
                .ToListAsync();

            return new PagedResultDto<AssetListItemDto>
            {
                Items = assets,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AssetDetailDto?> GetAssetDetailAsync(int assetId)
        {
            var asset = await _context.Assets
                .Include(a => a.Tenant)
                .FirstOrDefaultAsync(a => a.Id == assetId);

            if (asset == null) return null;

            var tickets = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Asset)
                .Where(t => t.AssetId == assetId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return new AssetDetailDto
            {
                Id = asset.Id,
                ProductName = asset.ProductName,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status,
                WarrantyEndDate = asset.WarrantyEndDate,
                IsUnderWarranty = asset.WarrantyEndDate > DateTimeHelper.GetLocalNow(),
                DaysUntilWarrantyExpires = (int)(asset.WarrantyEndDate - DateTimeHelper.GetLocalNow()).TotalDays,
                TenantId = asset.TenantId,
                TenantName = asset.Tenant?.CompanyName ?? "Bilinmeyen",
                TenantEmail = asset.Tenant?.Email ?? "",
                TotalTickets = tickets.Count,
                OpenTickets = tickets.Count(t => t.Status != TicketStatus.Resolved),
                ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                TicketHistory = tickets.Select(t => MapToTicketListItem(t)).ToList()
            };
        }

        // ==================== TICKET  ====================
                
        public async Task<PagedResultDto<TicketListItemDto>> GetTicketsAsync(TicketFilterDto filter)
        {
            var query = _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Asset)
                .AsQueryable();

            
            if (filter.TenantId.HasValue)
                query = query.Where(t => t.TenantId == filter.TenantId.Value);

            if (filter.Status.HasValue)
                query = query.Where(t => (int)t.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(t => (int)t.Priority == filter.Priority.Value);

            if (!string.IsNullOrWhiteSpace(filter.AssignedPerson))
                query = query.Where(t => t.AssignedPerson == filter.AssignedPerson);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedDate <= filter.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(t => t.Title.Contains(filter.SearchTerm) || t.Description.Contains(filter.SearchTerm));

            var totalCount = await query.CountAsync();

            
            query = filter.SortBy?.ToLower() switch
            {
                "priority" => filter.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                "status" => filter.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "title" => filter.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                _ => filter.SortDescending ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate)
            };

            var tickets = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResultDto<TicketListItemDto>
            {
                Items = tickets.Select(t => MapToTicketListItem(t)).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<TicketDetailDto?> GetTicketDetailAsync(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return null;

            var comments = await _context.TicketComments
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            var history = await _context.TicketHistories
                .Where(h => h.TicketId == ticketId)
                .OrderByDescending(h => h.CreatedDate)
                .ToListAsync();

            return new TicketDetailDto
            {
                Id = ticket.Id,
                TicketCode = ticket.TicketCode,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = GetStatusName(ticket.Status),
                Priority = GetPriorityName(ticket.Priority),
                AssignedPerson = ticket.AssignedPerson,
                ImagePath = ticket.ImagePath,
                CreatedDate = ticket.CreatedDate,
                UpdatedDate = ticket.UpdatedDate,
                TenantId = ticket.TenantId,
                TenantName = ticket.Tenant?.CompanyName ?? "Bilinmeyen",
                TenantEmail = ticket.Tenant?.Email ?? "",
                TenantPhone = ticket.Tenant?.PhoneNumber,
                AssetId = ticket.AssetId,
                AssetName = ticket.Asset?.ProductName,
                AssetSerialNumber = ticket.Asset?.SerialNumber,
                AssetUnderWarranty = ticket.Asset != null ? ticket.Asset.WarrantyEndDate > DateTimeHelper.GetLocalNow() : null,
                Comments = comments.Select(c => new TicketCommentDto
                {
                    Id = c.Id,
                    Message = c.Message,
                    AuthorName = c.AuthorName,
                    IsAdminReply = c.IsAdminReply,
                    CreatedDate = c.CreatedDate
                }).ToList(),
                History = history.Select(h => new TicketHistoryItemDto
                {
                    Description = h.Description,
                    ActionBy = h.ActionBy,
                    CreatedDate = h.CreatedDate
                }).ToList()
            };
        }

        // ==================== RAPORLAR ====================

        public async Task<WarrantyReportDto> GetWarrantyReportAsync()
        {
            var assets = await _context.Assets.Include(a => a.Tenant).ToListAsync();
            var now = DateTimeHelper.GetLocalNow();
            var thirtyDaysLater = now.AddDays(30);

            return new WarrantyReportDto
            {
                TotalAssets = assets.Count,
                UnderWarranty = assets.Count(a => a.WarrantyEndDate > now),
                ExpiredWarranty = assets.Count(a => a.WarrantyEndDate <= now),
                ExpiringSoon = assets.Count(a => a.WarrantyEndDate > now && a.WarrantyEndDate <= thirtyDaysLater),
                ExpiringAssets = assets
                    .Where(a => a.WarrantyEndDate > now && a.WarrantyEndDate <= thirtyDaysLater)
                    .OrderBy(a => a.WarrantyEndDate)
                    .Select(a => new AssetWarrantyAlertDto
                    {
                        Id = a.Id,
                        ProductName = a.ProductName,
                        SerialNumber = a.SerialNumber,
                        TenantName = a.Tenant?.CompanyName ?? "Bilinmeyen",
                        WarrantyEndDate = a.WarrantyEndDate,
                        DaysRemaining = (int)(a.WarrantyEndDate - now).TotalDays
                    })
                    .ToList()
            };
        }

        public async Task<PerformanceReportDto> GetPerformanceReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var from = fromDate ?? DateTimeHelper.GetLocalNow().AddDays(-30);
            var to = toDate ?? DateTimeHelper.GetLocalNow();

            var tickets = await _context.Tickets
                .Where(t => t.CreatedDate >= from && t.CreatedDate <= to)
                .ToListAsync();

            var resolvedTickets = tickets.Where(t => t.Status == TicketStatus.Resolved).ToList();

            return new PerformanceReportDto
            {
                FromDate = from,
                ToDate = to,
                TotalTicketsCreated = tickets.Count,
                TotalTicketsResolved = resolvedTickets.Count,
                AverageResolutionTimeHours = resolvedTickets.Any() && resolvedTickets.All(t => t.UpdatedDate.HasValue)
                    ? resolvedTickets.Average(t => (t.UpdatedDate!.Value - t.CreatedDate).TotalHours)
                    : 0,
                DailyStats = Enumerable.Range(0, (int)(to - from).TotalDays + 1)
                    .Select(i => from.AddDays(i).Date)
                    .Select(date => new DailyTicketStatsDto
                    {
                        Date = date,
                        Created = tickets.Count(t => t.CreatedDate.Date == date),
                        Resolved = resolvedTickets.Count(t => t.UpdatedDate?.Date == date)
                    })
                    .ToList(),
                StaffPerformance = tickets
                    .Where(t => !string.IsNullOrEmpty(t.AssignedPerson))
                    .GroupBy(t => t.AssignedPerson!)
                    .Select(g => new StaffPerformanceDto
                    {
                        StaffName = g.Key,
                        AssignedTickets = g.Count(),
                        ResolvedTickets = g.Count(t => t.Status == TicketStatus.Resolved),
                        OpenTickets = g.Count(t => t.Status != TicketStatus.Resolved)
                    })
                    .OrderByDescending(x => x.ResolvedTickets)
                    .ToList()
            };
        }

        // ==================== YARDIMCI METODLAR ====================

        private TicketListItemDto MapToTicketListItem(Ticket ticket)
        {
            return new TicketListItemDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Status = (int)ticket.Status,
                Priority = (int)ticket.Priority,
                AssignedPerson = ticket.AssignedPerson,
                TenantName = ticket.Tenant?.CompanyName ?? "Bilinmeyen",
                TenantId = ticket.TenantId,
                AssetName = ticket.Asset?.ProductName,
                AssetId = ticket.AssetId,
                CreatedDate = ticket.CreatedDate,
                UpdatedDate = ticket.UpdatedDate,
                CommentCount = _context.TicketComments.Count(c => c.TicketId == ticket.Id),
                IsOverdue = ticket.Status != TicketStatus.Resolved && (DateTimeHelper.GetLocalNow() - ticket.CreatedDate).TotalHours > 48
            };
        }

        private static string GetStatusName(TicketStatus status) => status switch
        {
            TicketStatus.Open => "Open",
            TicketStatus.Processing => "Processing",
            TicketStatus.WaitingForCustomer => "Waiting",
            TicketStatus.Resolved => "Resolved",
            TicketStatus.Closed => "Closed",
            _ => "Unknown"
        };

        private static string GetPriorityName(TicketPriority priority) => priority switch
        {
            TicketPriority.Low => "Low",
            TicketPriority.Medium => "Medium",
            TicketPriority.High => "High",
            TicketPriority.Critical => "Critical",
            _ => "Unknown"
        };
    }
}
