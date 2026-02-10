using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using KobiMuhendislikTicket.Hubs;

namespace KobiMuhendislikTicket.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<TicketService> _logger;
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IHubContext<DashboardStatsHub> _dashboardHubContext;

        public TicketService(
            ITicketRepository ticketRepository, 
            ILogger<TicketService> logger, 
            IHubContext<CommentHub> hubContext,
            IHubContext<DashboardStatsHub> dashboardHubContext)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
            _hubContext = hubContext;
            _dashboardHubContext = dashboardHubContext;
        }

        public async Task<Result<Ticket>> GetTicketByIdAsync(Guid id)
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

                var tickets = await _ticketRepository.GetAllAsync();
                var totalCount = tickets.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var pagedTickets = tickets
                    .OrderByDescending(t => t.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var items = pagedTickets.Select(t => new TicketListItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = (int)t.Status,
                    Priority = (int)t.Priority,
                    TenantName = t.Tenant?.CompanyName ?? "Bilinmiyor",
                    TenantId = t.TenantId,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    AssignedPerson = t.AssignedPerson ?? "Atanmadı",
                    AssetName = t.Asset?.ProductName ?? "Bilgisayar/Yazılım",
                    AssetId = t.AssetId
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

        public async Task<Result<List<Ticket>>> GetTenantTicketsAsync(Guid tenantId)
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
                ticket.CreatedDate = DateTime.UtcNow;

                await _ticketRepository.AddAsync(ticket);
                await LogHistoryAsync(ticket.Id, "System", "Yeni ticket oluşturuldu");
                
                _logger.LogInformation("Yeni ticket oluşturuldu: {TicketId}", ticket.Id);
                
                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket oluşturma hatası");
                return Result.Failure("Ticket oluşturulurken bir hata oluştu");
            }
        }

        public async Task<Result> UpdateTicketStatusAsync(Guid ticketId, int newStatus)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                if (!Enum.IsDefined(typeof(TicketStatus), newStatus))
                    return Result.Failure("Geçersiz durum değeri");

                var oldStatus = ticket.Status;
                var newStatusEnum = (TicketStatus)newStatus;

                await _ticketRepository.UpdateStatusDirectlyAsync(ticketId, newStatusEnum);
                await LogHistoryAsync(ticketId, "Admin", 
                    $"Ticket durumu '{oldStatus}' → '{newStatusEnum}' olarak güncellendi");

                _logger.LogInformation("Ticket durumu güncellendi: {TicketId}, {OldStatus} → {NewStatus}", 
                    ticketId, oldStatus, newStatusEnum);

                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket durumu güncellenirken hata: {TicketId}", ticketId);
                return Result.Failure("Durum güncellenirken bir hata oluştu");
            }
        }

        public async Task<Result> UpdateTicketPriorityAsync(Guid ticketId, int newPriority)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                if (!Enum.IsDefined(typeof(TicketPriority), newPriority))
                    return Result.Failure("Geçersiz öncelik değeri");

                var oldPriority = ticket.Priority;
                var newPriorityEnum = (TicketPriority)newPriority;

                await _ticketRepository.UpdatePriorityDirectlyAsync(ticketId, newPriorityEnum);
                await LogHistoryAsync(ticketId, "Admin", 
                    $"Öncelik '{oldPriority}' → '{newPriorityEnum}' olarak güncellendi");

                _logger.LogInformation("Ticket önceliği güncellendi: {TicketId}, {OldPriority} → {NewPriority}", 
                    ticketId, oldPriority, newPriorityEnum);

                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Öncelik güncellenirken hata: {TicketId}", ticketId);
                return Result.Failure("Öncelik güncellenirken bir hata oluştu");
            }
        }

        public async Task<Result> AssignTicketToPersonAsync(Guid ticketId, string personName)
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
                ticket.UpdatedDate = DateTime.UtcNow;

                await _ticketRepository.UpdateAsync(ticket);
                await LogHistoryAsync(ticketId, "Admin", $"Ticket '{personName}' personeline atandı");

                _logger.LogInformation("Ticket atandı: {TicketId} → {PersonName}", ticketId, personName);
                
                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket atama hatası: {TicketId}", ticketId);
                return Result.Failure("Ticket atanırken bir hata oluştu");
            }
        }

        public async Task<Result> ResolveTicketAsync(Guid ticketId, string solutionNote, string resolvedBy)
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
                ticket.UpdatedDate = DateTime.UtcNow;

                await _ticketRepository.UpdateAsync(ticket);
                await LogHistoryAsync(ticketId, resolvedBy, $"✅ TICKET ÇÖZÜLDÜ | Not: {solutionNote}");

                _logger.LogInformation("Ticket çözüldü: {TicketId} by {ResolvedBy}", ticketId, resolvedBy);
                
                // Dashboard istatistiklerini anlık olarak güncelle
                await BroadcastDashboardStatsAsync();
                
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
                    ProcessingTickets = tickets.Count(t => t.Status == TicketStatus.Processing),
                    ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
                    TotalTenants = totalTenants,
                    TotalAssets = totalAssets,
                    CriticalTicketCount = tickets.Count(t => t.Priority == TicketPriority.Critical && t.Status != TicketStatus.Resolved),
                    TopFailingAssets = tickets
                        .Where(t => t.Asset != null)
                        .GroupBy(t => t.Asset!.ProductName)
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

        public async Task<Result<List<Ticket>>> GetFilteredTicketsAsync(Guid? tenantId = null, TicketStatus? status = null, TicketPriority? priority = null, string? assignedPerson = null)
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

        public async Task<Result> AddCommentAsync(Guid ticketId, string message, string author, bool isAdmin, string source = "Customer")
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
                    CreatedDate = DateTime.UtcNow
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
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum eklenirken hata: {TicketId}", ticketId);
                return Result.Failure("Yorum eklenirken bir hata oluştu");
            }
        }

        private async Task LogHistoryAsync(Guid ticketId, string actionBy, string description)
        {
            try
            {
                var history = new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = actionBy,
                    Description = description,
                    CreatedDate = DateTime.UtcNow
                };

                await _ticketRepository.AddHistoryAsync(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geçmiş kaydı eklenirken hata: {TicketId}", ticketId);
            }
        }

        // Staff için overload metodlar
        public async Task<Result> AddCommentAsync(Guid ticketId, AddCommentDto dto)
        {
            return await AddCommentAsync(ticketId, dto.Message, dto.Author, dto.IsAdmin, "Admin");
        }

        public async Task<Result> ResolveTicketAsync(Guid ticketId, ResolveTicketDto dto)
        {
            return await ResolveTicketAsync(ticketId, dto.SolutionNote, dto.ResolvedBy);
        }

        public async Task<Result> UpdateStatusAsync(Guid ticketId, UpdateTicketStatusDto dto, string actionBy)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                if (!Enum.IsDefined(typeof(TicketStatus), dto.NewStatus))
                    return Result.Failure("Geçersiz durum değeri");

                var oldStatus = ticket.Status;
                var newStatusEnum = (TicketStatus)dto.NewStatus;

                await _ticketRepository.UpdateStatusDirectlyAsync(ticketId, newStatusEnum);
                await LogHistoryAsync(ticketId, actionBy, 
                    $"Ticket durumu '{oldStatus}' → '{newStatusEnum}' olarak güncellendi");

                _logger.LogInformation("Ticket durumu güncellendi: {TicketId}, {OldStatus} → {NewStatus}", 
                    ticketId, oldStatus, newStatusEnum);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket durumu güncellenirken hata: {TicketId}", ticketId);
                return Result.Failure("Durum güncellenirken bir hata oluştu");
            }
        }

        public async Task<List<TicketHistory>> GetTicketHistoryAsync(Guid ticketId)
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

        public async Task<List<TicketComment>> GetCommentsAsync(Guid ticketId)
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

        public async Task<Result> SaveTicketImageAsync(Guid ticketId, string imagePath)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı");

                ticket.ImagePath = imagePath;
                ticket.UpdatedDate = DateTime.UtcNow;
                
                await _ticketRepository.UpdateAsync(ticket);
                await LogHistoryAsync(ticketId, "System", "Ticket'a resim eklendi");

                _logger.LogInformation("Ticket'a resim eklendi: {TicketId}, {ImagePath}", ticketId, imagePath);
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