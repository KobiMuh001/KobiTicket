using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using KobiMuhendislikTicket.Hubs;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KobiMuhendislikTicket.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<TicketService> _logger;
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IHubContext<DashboardStatsHub> _dashboardHubContext;
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public TicketService(
            ITicketRepository ticketRepository, 
            ILogger<TicketService> logger, 
            IHubContext<CommentHub> hubContext,
            IHubContext<DashboardStatsHub> dashboardHubContext,
            ApplicationDbContext context,
            NotificationService notificationService)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
            _hubContext = hubContext;
            _dashboardHubContext = dashboardHubContext;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Result<Ticket>> GetTicketByIdAsync(int id)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(id);
                if (ticket == null)
                    return Result<Ticket>.Failure("Ticket bulunamadı");

                return Result<Ticket>.Success(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket getirme hatası: {TicketId}", id);
                return Result<Ticket>.Failure("Ticket getirilirken bir hata oluştu");
            }
        }

        public async Task<Result<List<Ticket>>> GetAllTicketsAsync()
        {
            try
            {
                var tickets = await _ticketRepository.GetAllAsync();
                return Result<List<Ticket>>.Success(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm ticket'lar getirilirken hata oluştu");
                return Result<List<Ticket>>.Failure("Ticket'lar yüklenirken bir hata oluştu");
            }
        }

        public async Task<Result<PaginatedTicketsDto>> GetAllTicketsPagedAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var (tickets, totalCount) = await _ticketRepository.GetAllTicketsPagedAsync(pageNumber, pageSize);
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var items = tickets.Select(t => new TicketListItemDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    Title = t.Title,
                    Status = (int)t.Status,
                    Priority = (int)t.Priority,
                    TenantName = t.Tenant?.CompanyName ?? "Bilinmiyor",
                    TenantId = t.TenantId,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    AssignedPerson = t.AssignedPerson ?? "Atanmadı",
                    ProductName = t.Product?.Name ?? "Belirtilmemiş",
                    ProductId = t.ProductId
                }).ToList();

                var result = new PaginatedTicketsDto
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                return Result<PaginatedTicketsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sayfalı ticket'lar getirilirken hata oluştu");
                return Result<PaginatedTicketsDto>.Failure("Ticket'lar yüklenirken bir hata oluştu");
            }
        }

        public async Task<Result<List<Ticket>>> GetTenantTicketsAsync(int tenantId)
        {
            try
            {
                var tickets = await _ticketRepository.GetByTenantIdAsync(tenantId);
                return Result<List<Ticket>>.Success(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firma ticket'ları getirilirken hata: {TenantId}", tenantId);
                return Result<List<Ticket>>.Failure("Firma ticket'ları yüklenirken hata oluştu");
            }
        }

        public async Task<Result> CreateTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Status = TicketStatus.Open;
                ticket.CreatedDate = DateTimeHelper.GetLocalNow();

                await _ticketRepository.AddAsync(ticket);
                await _context.SaveChangesAsync(); // İlk SaveChanges - ID'yi al
                
                // TicketCode'ı oluştur (T00001 formatı)
                ticket.TicketCode = $"T{ticket.Id:D5}";
                await _ticketRepository.UpdateAsync(ticket);
                await _context.SaveChangesAsync(); // İkinci SaveChanges - TicketCode'ı kaydet
                
                await LogHistoryAsync(ticket.Id, "System", "Yeni ticket oluşturuldu");
                
                _logger.LogInformation("Yeni ticket oluşturuldu: {TicketId}/{TicketCode}", ticket.Id, ticket.TicketCode);
                
                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                await BroadcastTicketUpdateAsync(ticket.Id);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket oluşturma hatası");
                return Result.Failure("Ticket oluşturulurken bir hata oluştu");
            }
        }

        public async Task<Result> UpdateTicketStatusAsync(int ticketId, int newStatus)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                var oldStatus = ticket.Status;
                // Allow numeric values from DB-driven lists; cast even if not defined in enum
                var newStatusEnum = (TicketStatus)newStatus;

                await _ticketRepository.UpdateStatusDirectlyAsync(ticketId, newStatusEnum);
                // Resolve friendly labels for history (prefer NumericKey -> SortOrder -> Id)
                var oldStatusValue = (int)oldStatus;
                var newStatusValue = newStatus;
                var oldStatusLabel = await ResolveLookupLabelAsync("TicketStatus", oldStatusValue) ?? oldStatus.ToString();
                var newStatusLabel = await ResolveLookupLabelAsync("TicketStatus", newStatusValue) ?? newStatusEnum.ToString();

                await LogHistoryAsync(ticketId, "Admin",
                    $"Ticket durumu '{oldStatusLabel}' → '{newStatusLabel}' olarak güncellendi");

                // Customer'a sadece status değişikliği bildirimi (lookup değeri gönderilsin)
                await _notificationService.NotifyCustomerStatusChangedAsync(
                    ticket.TenantId,
                    ticketId,
                    ticket.Title,
                    newStatusLabel
                );

                _logger.LogInformation("Ticket durumu güncellendi: {TicketId}, {OldStatus} → {NewStatus}", 
                    ticketId, oldStatus, newStatusEnum);

                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                await BroadcastTicketUpdateAsync(ticketId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket durumu güncellenirken hata: {TicketId}", ticketId);
                return Result.Failure("Durum güncellenirken bir hata oluştu");
            }
        }

        public async Task<Result> UpdateTicketPriorityAsync(int ticketId, int newPriority)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                var oldPriority = ticket.Priority;
                // Allow numeric values from DB-driven lists; cast even if not defined in enum
                var newPriorityEnum = (TicketPriority)newPriority;

                await _ticketRepository.UpdatePriorityDirectlyAsync(ticketId, newPriorityEnum);
                // Resolve friendly labels for history (prefer NumericKey -> SortOrder -> Id)
                var oldPriorityValue = (int)oldPriority;
                var newPriorityValue = newPriority;
                var oldPriorityLabel = await ResolveLookupLabelAsync("TicketPriority", oldPriorityValue) ?? oldPriority.ToString();
                var newPriorityLabel = await ResolveLookupLabelAsync("TicketPriority", newPriorityValue) ?? newPriorityEnum.ToString();

                await LogHistoryAsync(ticketId, "Admin",
                    $"Öncelik '{oldPriorityLabel}' → '{newPriorityLabel}' olarak güncellendi");

                _logger.LogInformation("Ticket önceliği güncellendi: {TicketId}, {OldPriority} → {NewPriority}", 
                    ticketId, oldPriority, newPriorityEnum);

                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                await BroadcastTicketUpdateAsync(ticketId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Öncelik güncellenirken hata: {TicketId}", ticketId);
                return Result.Failure("Öncelik güncellenirken bir hata oluştu");
            }
        }

        public async Task<Result> AssignTicketToPersonAsync(int ticketId, string personName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(personName))
                    return Result.Failure("Personel adı boş olamaz");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                ticket.AssignedPerson = personName;
                ticket.Status = TicketStatus.Processing;
                ticket.UpdatedDate = DateTimeHelper.GetLocalNow();

                await _ticketRepository.UpdateAsync(ticket);
                await LogHistoryAsync(ticketId, "Admin", $"Ticket '{personName}' personeline atandı");

                _logger.LogInformation("Ticket atandı: {TicketId} → {PersonName}", ticketId, personName);
                
                // Personel bilgisini bul
                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.FullName == personName || s.Email == personName);
                if (staff != null)
                {
                    // Tenant adını al
                    var tenant = await _context.Tenants.FindAsync(ticket.TenantId);
                    var tenantName = tenant?.CompanyName ?? "Müşteri";
                    
                    // Staff'a bildirim gönder
                    await _notificationService.NotifyTicketAssignedToStaffAsync(staff.Id, ticket.Title, tenantName, ticketId);
                }
                
                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                await BroadcastTicketUpdateAsync(ticketId);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket atama hatası: {TicketId}", ticketId);
                return Result.Failure("Ticket atanırken bir hata oluştu");
            }
        }

        public async Task<Result> ResolveTicketAsync(int ticketId, string solutionNote, string resolvedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(solutionNote))
                    return Result.Failure("Çözüm notu boş olamaz");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                if (ticket.Status == TicketStatus.Resolved)
                    return Result.Failure("Ticket zaten çözülmüş durumda");

                ticket.Status = TicketStatus.Resolved;
                ticket.UpdatedDate = DateTimeHelper.GetLocalNow();

                await _ticketRepository.UpdateAsync(ticket);
                await LogHistoryAsync(ticketId, resolvedBy, $"✅ TICKET ÇÖZÜLDÜ | Not: {solutionNote}");

                _logger.LogInformation("Ticket çözüldü: {TicketId} by {ResolvedBy}", ticketId, resolvedBy);
                
                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                await BroadcastTicketUpdateAsync(ticketId);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket çözülürken hata: {TicketId}", ticketId);
                return Result.Failure("Ticket çözülürken bir hata oluştu");
            }
        }

        public async Task<Result<DashboardStatsDto>> GetAdminDashboardStatsAsync()
        {
            try
            {
                var tickets = await _ticketRepository.GetAllAsync();
                var totalTenants = await _ticketRepository.GetTotalTenantsCountAsync();
                var totalAssets = await _ticketRepository.GetTotalAssetsCountAsync();

                var stats = new DashboardStatsDto
                {
                    TotalTickets = tickets.Count,
                    OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
                    // Count any ticket that is NOT Resolved and NOT Closed and NOT Open as processing
                    ProcessingTickets = tickets.Count(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Open),
                    ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                    TotalTenants = totalTenants,
                    TotalAssets = totalAssets,
                    CriticalTicketCount = tickets.Count(t => t.Priority == TicketPriority.Critical && t.Status != TicketStatus.Resolved),
                    TopFailingAssets = tickets
                        .Where(t => t.Product != null)
                        .GroupBy(t => t.Product!.Name)
                        .Select(g => new AssetTicketCountDto { ProductName = g.Key, TicketCount = g.Count() })
                        .OrderByDescending(x => x.TicketCount)
                        .Take(5).ToList()
                };

                return Result<DashboardStatsDto>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard istatistikleri alınırken hata");
                return Result<DashboardStatsDto>.Failure("İstatistikler yüklenirken hata oluştu");
            }
        }

        
        public async Task BroadcastDashboardStatsAsync()
        {
            try
            {
                var stats = await GetAdminDashboardStatsAsync();
                if (stats.IsSuccess && stats.Data != null)
                {
                    await _dashboardHubContext.Clients.Group("admin-dashboard")
                        .SendAsync("DashboardStatsUpdated", stats.Data);
                    _logger.LogInformation("Dashboard stats broadcast başarılı");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard stats broadcast sırasında hata");
            }
        }

        public async Task BroadcastTicketUpdateAsync(int ticketId)
        {
            try
            {
                var ticketResult = await GetTicketByIdAsync(ticketId);
                if (ticketResult.IsSuccess && ticketResult.Data != null)
                {
                    var ticket = ticketResult.Data;
                    var ticketDto = new TicketListItemDto
                    {
                        Id = ticket.Id,
                        TicketCode = ticket.TicketCode,
                        Title = ticket.Title,
                        Status = (int)ticket.Status,
                        Priority = (int)ticket.Priority,
                        TenantName = ticket.Tenant?.CompanyName ?? "Bilinmiyor",
                        TenantId = ticket.TenantId,
                        CreatedDate = ticket.CreatedDate,
                        UpdatedDate = ticket.UpdatedDate,
                        AssignedPerson = ticket.AssignedPerson ?? "Atanmadı",
                        ProductName = ticket.Product?.Name ?? "Belirtilmemiş",
                        ProductId = ticket.ProductId
                    };

                    await _hubContext.Clients.Group($"ticket-{ticketId}")
                        .SendAsync("TicketUpdated", ticketDto);
                    _logger.LogInformation("Ticket update broadcast başarılı: {TicketId}", ticketId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket update broadcast sırasında hata: {TicketId}", ticketId);
            }
        }

        public async Task<Result<List<Ticket>>> GetFilteredTicketsAsync(int? tenantId = null, TicketStatus? status = null, TicketPriority? priority = null, string? assignedPerson = null)
        {
            try
            {
                var tickets = await _ticketRepository.GetFilteredTicketsAsync(tenantId, status, priority, assignedPerson);
                return Result<List<Ticket>>.Success(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Filtrelenmiş ticket'lar getirilirken hata");
                return Result<List<Ticket>>.Failure("Ticket'lar filtrelenirken hata oluştu");
            }
        }

        public async Task<Result> AddCommentAsync(int ticketId, string message, string author, bool isAdmin, string source = "Customer")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    return Result.Failure("Yorum boş olamaz");

                var ticketExists = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticketExists == null)
                    return Result.Failure("Ticket bulunamadı");

                var comment = new TicketComment
                {
                    TicketId = ticketId,
                    Message = message,
                    AuthorName = author,
                    IsAdminReply = isAdmin,
                    CreatedDate = DateTimeHelper.GetLocalNow()
                };

                await _ticketRepository.AddCommentAsync(comment);
                
                // Kaynağa göre history mesajı
                string sourceLabel = source switch
                {
                    "Staff" => " Staff",
                    "Admin" => " Admin",
                    _ => " Müşteri"
                };
                await LogHistoryAsync(ticketId, author, $"{sourceLabel} yorum ekledi");

                // SignalR ile real-time bildirim gönder
                try
                {
                    var groupName = $"ticket-{ticketId}";
                    _logger.LogInformation("SignalR: Sending comment to group {GroupName}", groupName);
                    
                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveComment", new
                    {
                        id = comment.Id,
                        ticketId = ticketId,
                        message = message,
                        authorName = author,
                        isAdminReply = isAdmin,
                        createdDate = comment.CreatedDate
                    });
                    
                    _logger.LogInformation("SignalR: Comment sent successfully to group {GroupName}", groupName);
                }
                catch (Exception signalREx)
                {
                    _logger.LogError(signalREx, "SignalR: Error sending comment to group ticket-{TicketId}", ticketId);
                }

                _logger.LogInformation("Yorum eklendi: {TicketId} by {Author}", ticketId, author);

                // Customer'a sadece admin/staff tarafından gelen mesajları bildir
                if (!string.Equals(source, "Customer", System.StringComparison.OrdinalIgnoreCase))
                {
                    await _notificationService.NotifyCustomerNewCommentAsync(
                        ticketExists.TenantId,
                        ticketId,
                        ticketExists.Title,
                        author
                    );
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum eklenirken hata: {TicketId}", ticketId);
                return Result.Failure("Yorum eklenirken bir hata oluştu");
            }
        }

        private async Task LogHistoryAsync(int ticketId, string actionBy, string description)
        {
            try
            {
                var history = new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = actionBy,
                    Description = description,
                    CreatedDate = DateTimeHelper.GetLocalNow()
                };

                await _ticketRepository.AddHistoryAsync(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geçmiş kaydı eklenirken hata: {TicketId}", ticketId);
            }
        }

        // Resolve a user-friendly label for lookup groups (TicketStatus, TicketPriority, ...)
        // Preference order: NumericKey -> SortOrder -> Id. Returns null if not found.
        private async Task<string?> ResolveLookupLabelAsync(string group, int numericValue)
        {
            try
            {
                // Prefer matching NumericKey only. If not found, allow matching by Id as a last resort.
                var param = await _context.SystemParameters
                    .FirstOrDefaultAsync(p => p.Group == group && p.NumericKey.HasValue && p.NumericKey.Value == numericValue);

                if (param == null)
                {
                    param = await _context.SystemParameters
                        .FirstOrDefaultAsync(p => p.Group == group && p.Id == numericValue);
                }

                if (param != null)
                {
                    return param.Value ?? param.Key ?? numericValue.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lookup label resolution failed for group {Group} value {Value}", group, numericValue);
            }

            return null;
        }

        // Staff için overload metodlar
        public async Task<Result> AddCommentAsync(int ticketId, AddCommentDto dto)
        {
            return await AddCommentAsync(ticketId, dto.Message, dto.Author, dto.IsAdmin, "Admin");
        }

        public async Task<Result> ResolveTicketAsync(int ticketId, ResolveTicketDto dto)
        {
            return await ResolveTicketAsync(ticketId, dto.SolutionNote, dto.ResolvedBy);
        }

        public async Task<Result> UpdateStatusAsync(int ticketId, UpdateTicketStatusDto dto, string actionBy)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                // Allow numeric values from DB-driven lists; cast even if not defined in enum
                var oldStatus = ticket.Status;
                var newStatusEnum = (TicketStatus)dto.NewStatus;

                await _ticketRepository.UpdateStatusDirectlyAsync(ticketId, newStatusEnum);
                // Resolve friendly labels from SystemParameters (prefer NumericKey -> SortOrder -> Id)
                var oldStatusValue = (int)oldStatus;
                var newStatusValue = (int)newStatusEnum;
                var oldLabel = await ResolveLookupLabelAsync("TicketStatus", oldStatusValue) ?? oldStatus.ToString();
                var newLabel = await ResolveLookupLabelAsync("TicketStatus", newStatusValue) ?? newStatusEnum.ToString();

                await LogHistoryAsync(ticketId, actionBy,
                    $"Ticket durumu '{oldLabel}' → '{newLabel}' olarak güncellendi");

                // Customer'a sadece status değişikliği bildirimi
                await _notificationService.NotifyCustomerStatusChangedAsync(
                    ticket.TenantId,
                    ticketId,
                    ticket.Title,
                    newStatusEnum.ToString()
                );

                _logger.LogInformation("Ticket durumu güncellendi: {TicketId}, {OldStatus} → {NewStatus}", 
                    ticketId, oldStatus, newStatusEnum);

                await BroadcastDashboardStatsAsync();
                await BroadcastTicketUpdateAsync(ticketId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket durumu güncellenirken hata: {TicketId}", ticketId);
                return Result.Failure("Durum güncellenirken bir hata oluştu");
            }
        }

        public async Task<List<TicketHistory>> GetTicketHistoryAsync(int ticketId)
        {
            try
            {
                return await _ticketRepository.GetHistoryAsync(ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket geçmişi getirilirken hata: {TicketId}", ticketId);
                return new List<TicketHistory>();
            }
        }

        public async Task<List<TicketComment>> GetCommentsAsync(int ticketId)
        {
            try
            {
                return await _ticketRepository.GetCommentsAsync(ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket yorumları getirilirken hata: {TicketId}", ticketId);
                return new List<TicketComment>();
            }
        }

        public async Task<Result> SaveTicketImageAsync(int ticketId, string imagePath)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                if (string.IsNullOrWhiteSpace(ticket.ImagePath))
                {
                    ticket.ImagePath = imagePath;
                }

                var ticketImage = new TicketImage
                {
                    TicketId = ticketId,
                    ImagePath = imagePath
                };

                _context.TicketImages.Add(ticketImage);
                ticket.UpdatedDate = DateTimeHelper.GetLocalNow();
                await _context.SaveChangesAsync();
                await LogHistoryAsync(ticketId, "System", "Ticket'a resim eklendi");

                _logger.LogInformation("Ticket'a resim eklendi: {TicketId}, {ImagePath}", ticketId, imagePath);
                
                await BroadcastTicketUpdateAsync(ticketId);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket resmi kaydedilirken hata: {TicketId}", ticketId);
                return Result.Failure("Resim kaydedilirken bir hata oluştu");
            }
        }
    }
}