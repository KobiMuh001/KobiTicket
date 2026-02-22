using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;

namespace KobiMuhendislikTicket.Application.Services
{
    public class StaffService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<StaffService> _logger;
        private readonly NotificationService _notificationService;
        private readonly IHostEnvironment _env;
        private readonly ITicketService _ticketService;

        public StaffService(
            ApplicationDbContext context, 
            ITicketRepository ticketRepository, 
            ILogger<StaffService> logger, 
            NotificationService notificationService, 
            IHostEnvironment env, 
            ITicketService ticketService)
        {
            _context = context;
            _ticketRepository = ticketRepository;
            _logger = logger;
            _notificationService = notificationService;
            _env = env;
            _ticketService = ticketService;
        }

        

        public async Task<Result<Staff>> CreateStaffAsync(CreateStaffDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.FullName))
                    return Result<Staff>.Failure("Çalışan adı gereklidir.");

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return Result<Staff>.Failure("Email adresi gereklidir.");

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return Result<Staff>.Failure("Şifre gereklidir.");

                if (dto.Password.Length < 6)
                    return Result<Staff>.Failure("Şifre en az 6 karakter olmalıdır.");

               
                var existingEmail = await _context.Staff.AnyAsync(s => s.Email == dto.Email);
                if (existingEmail)
                    return Result<Staff>.Failure("Bu email adresi zaten kullanılıyor.");

                // Resolve department id from SystemParameters (Department group)
                var deptParam = await _context.SystemParameters.FirstOrDefaultAsync(p => p.Group == "Department" && (p.Id == dto.DepartmentId || p.NumericKey == dto.DepartmentId || p.Key == dto.DepartmentId.ToString()));
                if (deptParam == null)
                {
                    return Result<Staff>.Failure($"Department with id {dto.DepartmentId} not found in SystemParameters.");
                }

                var staff = new Staff
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Phone = dto.Phone,
                    DepartmentId = deptParam.Id,
                    MaxConcurrentTickets = dto.MaxConcurrentTickets,
                    IsActive = true
                };

                await _context.Staff.AddAsync(staff);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni çalışan oluşturuldu: {StaffName}", staff.FullName);
                return Result<Staff>.Success(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalışan oluşturulurken hata");
                if (_env != null && _env.IsDevelopment())
                {
                    return Result<Staff>.Failure(ex.InnerException?.Message ?? ex.Message);
                }
                return Result<Staff>.Failure("Çalışan oluşturulurken bir hata oluştu.");
            }
        }

        public async Task<Result> UpdateStaffAsync(int staffId, UpdateStaffDto dto)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                if (!string.IsNullOrWhiteSpace(dto.FullName) && dto.FullName != "string")
                    staff.FullName = dto.FullName;

                if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != "string")
                {
                    var emailExists = await _context.Staff.AnyAsync(s => s.Email == dto.Email && s.Id != staffId);
                    if (emailExists)
                        return Result.Failure("Bu email adresi başka bir çalışan tarafından kullanılıyor.");
                    staff.Email = dto.Email;
                }

                if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != "string")
                    staff.Phone = dto.Phone;

                if (dto.DepartmentId.HasValue)
                {
                    var param = await _context.SystemParameters.FirstOrDefaultAsync(p => p.Group == "Department" && (p.Id == dto.DepartmentId.Value || p.NumericKey == dto.DepartmentId.Value || p.Key == dto.DepartmentId.Value.ToString()));
                    if (param == null)
                        return Result.Failure("Belirtilen departman bulunamadı.");
                    staff.DepartmentId = param.Id;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Department) && dto.Department != "string")
                {
                    var param = await _context.SystemParameters.FirstOrDefaultAsync(p => p.Group == "Department" && (p.Key == dto.Department || p.Value == dto.Department));
                    if (param != null)
                        staff.DepartmentId = param.Id;
                }

                if (dto.IsActive.HasValue)
                    staff.IsActive = dto.IsActive.Value;

                if (dto.MaxConcurrentTickets.HasValue)
                    staff.MaxConcurrentTickets = dto.MaxConcurrentTickets.Value;

                if (!string.IsNullOrWhiteSpace(dto.NewPassword) && dto.NewPassword != "string")
                {
                    if (dto.NewPassword.Length < 6)
                        return Result.Failure("Şifre en az 6 karakter olmalıdır.");
                    staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                }

                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalışan güncellenirken hata: {StaffId}", staffId);
                return Result.Failure("Çalışan güncellenirken bir hata oluştu.");
            }
        }

        public async Task<Result> DeleteStaffAsync(int staffId)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("�al��an bulunamad�.");

                // Atanm�� a��k biletleri kontrol et
                var normalizedName = (staff.FullName ?? string.Empty).Trim().ToLower();
                var assignedTickets = await _context.Tickets
                    .CountAsync(t => !string.IsNullOrEmpty(t.AssignedPerson) && t.AssignedPerson.Trim().ToLower() == normalizedName && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed);

                if (assignedTickets > 0)
                    return Result.Failure($"Bu �al��an�n {assignedTickets} adet a��k ticket'� var. �nce ticket'lar� ba�ka birine atay�n.");

                _context.Staff.Remove(staff);
                await _context.SaveChangesAsync();

                _logger.LogInformation("�al��an silindi: {StaffName}", staff.FullName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "�al��an silinirken hata: {StaffId}", staffId);
                return Result.Failure("�al��an silinirken bir hata olu�tu.");
            }
        }

        

        public async Task<List<StaffDto>> GetAllStaffAsync(bool? activeOnly = null)
        {
            var query = _context.Staff.AsQueryable();

            if (activeOnly.HasValue)
                query = query.Where(s => s.IsActive == activeOnly.Value);

                return await query
                .OrderBy(s => s.FullName)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    DepartmentId = s.DepartmentId,
                    Department = s.Department != null ? s.Department.Value ?? string.Empty : string.Empty,
                    IsActive = s.IsActive,
                    MaxConcurrentTickets = s.MaxConcurrentTickets,
                    CreatedDate = s.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<StaffDto?> GetStaffByIdAsync(int staffId)
        {
            return await _context.Staff
                .Where(s => s.Id == staffId)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    DepartmentId = s.DepartmentId,
                    Department = s.Department != null ? s.Department.Value ?? string.Empty : string.Empty,
                    IsActive = s.IsActive,
                    MaxConcurrentTickets = s.MaxConcurrentTickets,
                    CreatedDate = s.CreatedDate
                })
                .FirstOrDefaultAsync();
        }

       

        public async Task<List<StaffWorkloadDto>> GetStaffWorkloadsAsync()
        {
            var staffList = await _context.Staff.Where(s => s.IsActive).ToListAsync();
            var tickets = await _context.Tickets.ToListAsync();
            var today = DateTimeHelper.GetLocalNow().Date;
            var weekAgo = today.AddDays(-7);

            var workloads = staffList.Select(staff =>
            {
                var staffTickets = tickets.Where(t => t.AssignedStaffId == staff.Id).ToList();
                var openTickets = staffTickets.Count(t => t.Status == TicketStatus.Open);
                var processingTickets = staffTickets.Count(t => t.Status == TicketStatus.Processing);
                // Consider any status that is not Resolved or Closed as active so new DB-added statuses are included
                var activeTickets = staffTickets.Count(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed);

                    return new StaffWorkloadDto
                    {
                        Id = staff.Id,
                        FullName = staff.FullName,
                        DepartmentId = staff.DepartmentId,
                        Department = staff.Department != null ? staff.Department.Value ?? string.Empty : string.Empty,
                        IsActive = staff.IsActive,
                        MaxConcurrentTickets = staff.MaxConcurrentTickets,
                        AssignedTickets = activeTickets,
                        OpenTickets = openTickets,
                        ProcessingTickets = processingTickets,
                        ResolvedToday = staffTickets.Count(t => t.Status == TicketStatus.Resolved && t.UpdatedDate?.Date == today),
                        ResolvedThisWeek = staffTickets.Count(t => t.Status == TicketStatus.Resolved && t.UpdatedDate >= weekAgo),
                        IsAvailable = activeTickets < staff.MaxConcurrentTickets,
                        WorkloadPercentage = staff.MaxConcurrentTickets > 0 
                            ? Math.Round((double)activeTickets / staff.MaxConcurrentTickets * 100, 1) 
                            : 0
                    };
            })
            .OrderBy(w => w.WorkloadPercentage)
            .ToList();

            return workloads;
        }

        

        public async Task<Result> AssignTicketToStaffAsync(int ticketId, int staffId, string? note = null)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("�al��an bulunamad�.");

                if (!staff.IsActive)
                    return Result.Failure("Bu �al��an aktif de�il.");

                
                var currentTickets = await _context.Tickets
                    .CountAsync(t => t.AssignedStaffId == staffId && 
                                    (t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed));

                if (currentTickets >= staff.MaxConcurrentTickets)
                    return Result.Failure($"{staff.FullName} maksimum ticket kapasitesine ula�m�� ({currentTickets}/{staff.MaxConcurrentTickets}).");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamad�.");

                var oldAssignee = ticket.AssignedPerson;
                ticket.AssignedStaffId = staff.Id;
                ticket.AssignedPerson = staff.FullName;
                ticket.Status = TicketStatus.Processing;
                ticket.UpdatedDate = DateTimeHelper.GetLocalNow();

                await _ticketRepository.UpdateAsync(ticket);

                
                var description = string.IsNullOrEmpty(oldAssignee)
                    ? $"Ticket '{staff.FullName}' personeline atand�"
                    : $"Ticket '{oldAssignee}' ? '{staff.FullName}' olarak yeniden atand�";

                if (!string.IsNullOrWhiteSpace(note))
                    description += $" | Not: {note}";

                await _ticketRepository.AddHistoryAsync(new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = "Admin",
                    Description = description,
                    CreatedDate = DateTimeHelper.GetLocalNow()
                });
                // Staff'a email bildirimi gönder
                var tenant = await _context.Tenants.FindAsync(ticket.TenantId);
                await _notificationService.NotifyTicketAssignedToStaffAsync(
                    staffId,
                    ticket.Title,
                    tenant?.CompanyName ?? "Müşteri",
                    ticketId
                );
                _logger.LogInformation("Ticket atand: {TicketId} ? {StaffName}", ticketId, staff.FullName);
                
                // Real-time broadcast
                await _ticketService.BroadcastDashboardStatsAsync();
                await _ticketService.BroadcastTicketUpdateAsync(ticketId);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket atanrken hata: {TicketId}", ticketId);
                return Result.Failure("Ticket atanrken bir hata olutu.");
            }
        }

        public async Task<Result<int>> BulkAssignTicketsAsync(List<int> ticketIds, int staffId)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result<int>.Failure("alan bulunamad.");

                if (!staff.IsActive)
                    return Result<int>.Failure("Bu alan aktif deil.");

                int successCount = 0;
                foreach (var ticketId in ticketIds)
                {
                    var result = await AssignTicketToStaffAsync(ticketId, staffId);
                    if (result.IsSuccess)
                        successCount++;
                }

                return Result<int>.Success(successCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu atama hatası");
                return Result<int>.Failure("Toplu atama sırasında hata oluştu.");
            }
        }

        public async Task<Result<AutoAssignResultDto>> AutoAssignTicketAsync(int ticketId)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result<AutoAssignResultDto>.Failure("Ticket bulunamadı.");

                if (!string.IsNullOrEmpty(ticket.AssignedPerson))
                    return Result<AutoAssignResultDto>.Failure("Bu ticket zaten birine atanmış.");

                
                var workloads = await GetStaffWorkloadsAsync();
                var availableStaff = workloads
                    .Where(w => w.IsAvailable && w.IsActive)
                    .OrderBy(w => w.WorkloadPercentage)
                    .FirstOrDefault();

                if (availableStaff == null)
                    return Result<AutoAssignResultDto>.Failure("Müsait alan bulunamadı.");

                var assignResult = await AssignTicketToStaffAsync(ticketId, availableStaff.Id, "Otomatik atama");
                if (!assignResult.IsSuccess)
                    return Result<AutoAssignResultDto>.Failure(assignResult.ErrorMessage!);

                return Result<AutoAssignResultDto>.Success(new AutoAssignResultDto
                {
                    Success = true,
                    Message = $"Ticket otomatik olarak atandı",
                    AssignedTo = availableStaff.FullName,
                    StaffId = availableStaff.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik atama hatası: {TicketId}", ticketId);
                return Result<AutoAssignResultDto>.Failure("Otomatik atama sırasında hata olutu.");
            }
        }

        

        public async Task<Result> UnassignTicketAsync(int ticketId)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamad.");

                var oldAssignee = ticket.AssignedPerson;
                if (!ticket.AssignedStaffId.HasValue && string.IsNullOrEmpty(oldAssignee))
                    return Result.Failure("Bu ticket zaten kimseye atanmamış.");

                ticket.AssignedStaffId = null;
                ticket.AssignedPerson = null;
                ticket.Status = TicketStatus.Open;
                ticket.UpdatedDate = DateTimeHelper.GetLocalNow();

                await _ticketRepository.UpdateAsync(ticket);

                await _ticketRepository.AddHistoryAsync(new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = "Admin",
                    Description = $"Ticket '{oldAssignee}' personelinden geri alındı",
                    CreatedDate = DateTimeHelper.GetLocalNow()
                });

                _logger.LogInformation("Ticket ataması kaldırıldı: {TicketId}", ticketId);

                // Real-time broadcast
                await _ticketService.BroadcastDashboardStatsAsync();
                await _ticketService.BroadcastTicketUpdateAsync(ticketId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Atama kaldırılırken hata: {TicketId}", ticketId);
                return Result.Failure("Atama kaldırılırken bir hata oluştu.");
            }
        }

        // Staff Login Validation
        public async Task<Staff?> ValidateStaffLoginAsync(string email, string password)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.Email == email);
            if (staff == null || !staff.IsActive)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, staff.PasswordHash))
                return null;

            return staff;
        }

        // Admin şifre sıfırlama
        public async Task<Result> ResetStaffPasswordAsync(int staffId, string newPassword)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                    return Result.Failure("Şifre en az 6 karakter olmalıdır.");

                staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Çalışan şifresi sıfırlandı: {StaffId}", staffId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlanırken hata: {StaffId}", staffId);
                return Result.Failure("Şifre sıfırlanırken bir hata oluştu.");
            }
        }

        // Staff'ın kendisine atanmış ticketları getir
        public async Task<List<TicketDto>> GetMyTicketsAsync(int staffId, int page = 1, int pageSize = 20)
        {
            var staff = await _context.Staff.FindAsync(staffId);
            if (staff == null)
                return new List<TicketDto>();

            var normalizedName = (staff.FullName ?? string.Empty).Trim().ToLower();
            var tickets = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Product)
                .Where(t => t.AssignedStaffId == staffId)
                .OrderBy(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed ? 1 : 0)
                .ThenByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssignedPerson = t.AssignedPerson,
                    AssignedStaffId = t.AssignedStaffId,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    TenantId = t.TenantId,
                    CompanyName = t.Tenant != null ? t.Tenant.CompanyName : null,
                    ProductId = t.ProductId,
                    ProductName = t.Product != null ? t.Product.Name : null,
                    ImagePath = t.ImagePath
                })
                .ToListAsync();

            return tickets;
        }

        // Tüm ticketları getir (pagination olmadan - iç kontroller için)
        public async Task<List<TicketDto>> GetAllMyTicketsAsync(int staffId)
        {
            var staff = await _context.Staff.FindAsync(staffId);
            if (staff == null)
                return new List<TicketDto>();

            var normalizedName = (staff.FullName ?? string.Empty).Trim().ToLower();
            var tickets = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Product)
                .Where(t => t.AssignedStaffId == staffId)
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssignedPerson = t.AssignedPerson,
                    AssignedStaffId = t.AssignedStaffId,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    TenantId = t.TenantId,
                    CompanyName = t.Tenant != null ? t.Tenant.CompanyName : null,
                    ProductId = t.ProductId,
                    ProductName = t.Product != null ? t.Product.Name : null,
                    ImagePath = t.ImagePath
                })
                .ToListAsync();

            return tickets;
        }

        // Atanmamış (boş) ticketları getir
        public async Task<List<TicketDto>> GetUnassignedTicketsAsync()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Product)
                // Return tickets that are not assigned to anyone regardless of their status
                .Where(t => !t.AssignedStaffId.HasValue)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.CreatedDate)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssignedPerson = t.AssignedPerson,
                    AssignedStaffId = t.AssignedStaffId,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    TenantId = t.TenantId,
                    CompanyName = t.Tenant != null ? t.Tenant.CompanyName : null,
                    ProductId = t.ProductId,
                    ProductName = t.Product != null ? t.Product.Name : null,
                    ImagePath = t.ImagePath
                })
                .ToListAsync();

            return tickets;
        }

        // Staff ticket'ı kendine atasın
        public async Task<Result> ClaimTicketAsync(int ticketId, int staffId)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                if (!staff.IsActive)
                    return Result.Failure("Hesabınız aktif değil.");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı.");

                if (ticket.AssignedStaffId.HasValue)
                    return Result.Failure("Bu ticket zaten birine atanmış.");

                // Kapasite kontrolü
                var currentTickets = await _context.Tickets
                    .CountAsync(t => t.AssignedStaffId == staff.Id && 
                                    (t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed));

                if (currentTickets >= staff.MaxConcurrentTickets)
                    return Result.Failure($"Maksimum ticket kapasitesine ulaştınız ({currentTickets}/{staff.MaxConcurrentTickets}).");

                ticket.AssignedStaffId = staff.Id;
                ticket.AssignedPerson = staff.FullName;
                ticket.Status = TicketStatus.Processing;
                ticket.UpdatedDate = DateTimeHelper.GetLocalNow();

                await _ticketRepository.UpdateAsync(ticket);

                await _ticketRepository.AddHistoryAsync(new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = staff.FullName,
                    Description = $"Ticket '{staff.FullName}' tarafından alındı",
                    CreatedDate = DateTimeHelper.GetLocalNow()
                });

                _logger.LogInformation("Ticket alındı: {TicketId} -> {StaffName}", ticketId, staff.FullName);

                // Real-time broadcast
                await _ticketService.BroadcastDashboardStatsAsync();
                await _ticketService.BroadcastTicketUpdateAsync(ticketId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket alınırken hata: {TicketId}", ticketId);
                return Result.Failure("Ticket alınırken bir hata oluştu.");
            }
        }

        // Staff ticket'ı bıraksın (unassign from self)
        public async Task<Result> ReleaseTicketAsync(int ticketId, int staffId)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı.");

                if (ticket.AssignedStaffId != staffId)
                    return Result.Failure("Bu ticket size atanmamış.");

                ticket.AssignedStaffId = null;
                ticket.AssignedPerson = null;
                ticket.Status = TicketStatus.Open;
                ticket.UpdatedDate = DateTimeHelper.GetLocalNow();

                await _ticketRepository.UpdateAsync(ticket);

                await _ticketRepository.AddHistoryAsync(new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = staff.FullName,
                    Description = $"Ticket '{staff.FullName}' tarafından bırakıldı",
                    CreatedDate = DateTimeHelper.GetLocalNow()
                });

                _logger.LogInformation("Ticket bırakıldı: {TicketId} <- {StaffName}", ticketId, staff.FullName);

                // Real-time broadcast
                await _ticketService.BroadcastDashboardStatsAsync();
                await _ticketService.BroadcastTicketUpdateAsync(ticketId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket bırakılırken hata: {TicketId}", ticketId);
                return Result.Failure("Ticket bırakılırken bir hata oluştu.");
            }
        }

        // Staff profilini getir
        public async Task<StaffDto?> GetStaffProfileAsync(int staffId)
        {
            return await _context.Staff
                .Where(s => s.Id == staffId)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    DepartmentId = s.DepartmentId,
                    Department = s.Department != null ? s.Department.Value ?? string.Empty : string.Empty,
                    IsActive = s.IsActive,
                    MaxConcurrentTickets = s.MaxConcurrentTickets,
                    CreatedDate = s.CreatedDate
                })
                .FirstOrDefaultAsync();
        }

        // Staff workload özeti
        public async Task<StaffWorkloadDto?> GetMyWorkloadAsync(int staffId)
        {
            var staff = await _context.Staff.FindAsync(staffId);
            if (staff == null)
                return null;

            var today = DateTimeHelper.GetLocalNow().Date;
            var weekAgo = today.AddDays(-7);

            var staffTickets = await _context.Tickets
                .Where(t => t.AssignedStaffId == staffId)
                .ToListAsync();

            var openTickets = staffTickets.Count(t => t.Status == TicketStatus.Open);
            var processingTickets = staffTickets.Count(t => t.Status == TicketStatus.Processing);
            // Treat any status that is not Resolved or Closed as active so DB-added statuses are included
            var activeTickets = staffTickets.Count(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed);

            return new StaffWorkloadDto
            {
                Id = staff.Id,
                FullName = staff.FullName,
                DepartmentId = staff.DepartmentId,
                Department = staff.Department != null ? staff.Department.Value ?? string.Empty : string.Empty,
                IsActive = staff.IsActive,
                MaxConcurrentTickets = staff.MaxConcurrentTickets,
                AssignedTickets = activeTickets,
                OpenTickets = openTickets,
                ProcessingTickets = processingTickets,
                ResolvedToday = staffTickets.Count(t => t.Status == TicketStatus.Resolved && t.UpdatedDate?.Date == today),
                ResolvedThisWeek = staffTickets.Count(t => t.Status == TicketStatus.Resolved && t.UpdatedDate >= weekAgo),
                IsAvailable = activeTickets < staff.MaxConcurrentTickets,
                WorkloadPercentage = staff.MaxConcurrentTickets > 0 
                    ? Math.Round((double)activeTickets / staff.MaxConcurrentTickets * 100, 1) 
                    : 0
            };
        }

        // Staff kendi profilini güncelle
        public async Task<Result> UpdateOwnProfileAsync(int staffId, UpdateOwnProfileDto dto)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                if (!string.IsNullOrWhiteSpace(dto.FullName) && dto.FullName != "string")
                    staff.FullName = dto.FullName;

                if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != "string")
                    staff.Phone = dto.Phone;

                if (dto.DepartmentId.HasValue)
                {
                    var param = await _context.SystemParameters.FirstOrDefaultAsync(p => p.Group == "Department" && (p.Id == dto.DepartmentId.Value || p.NumericKey == dto.DepartmentId.Value || p.Key == dto.DepartmentId.Value.ToString()));
                    if (param == null)
                        return Result.Failure("Belirtilen departman bulunamadı.");
                    staff.DepartmentId = param.Id;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Department) && dto.Department != "string")
                {
                    var param = await _context.SystemParameters.FirstOrDefaultAsync(p => p.Group == "Department" && (p.Key == dto.Department || p.Value == dto.Department));
                    if (param != null)
                        staff.DepartmentId = param.Id;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Çalışan profili güncellendi: {StaffId}", staffId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kendi profil güncellenirken hata: {StaffId}", staffId);
                return Result.Failure("Profil güncellenirken bir hata oluştu.");
            }
        }

        // Staff kendi şifresini değiştir
        public async Task<Result> ChangeOwnPasswordAsync(int staffId, ChangePasswordDto dto)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                // Eski şifreyi doğrula
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, staff.PasswordHash))
                    return Result.Failure("Mevcut şifre yanlış.");

                // Yeni şifre validasyonu
                if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                    return Result.Failure("Yeni şifre en az 6 karakter olmalıdır.");

                // Büyük harf kontrolü
                if (!Regex.IsMatch(dto.NewPassword, "[A-Z]"))
                    return Result.Failure("Şifre en az bir büyük harf içermelidir.");

                // Küçük harf kontrolü
                if (!Regex.IsMatch(dto.NewPassword, "[a-z]"))
                    return Result.Failure("Şifre en az bir küçük harf içermelidir.");

                // Sayı kontrolü
                if (!Regex.IsMatch(dto.NewPassword, "[0-9]"))
                    return Result.Failure("Şifre en az bir sayı içermelidir.");

                staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Çalışan şifresi değiştirildi: {StaffId}", staffId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirilirken hata: {StaffId}", staffId);
                return Result.Failure("Şifre değiştirilirken bir hata oluştu.");
            }
        }    }
}