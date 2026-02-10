using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Domain.Enums;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KobiMuhendislikTicket.Application.Services
{
    public class StaffService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITicketRepository _ticketRepository;
        private readonly ILogger<StaffService> _logger;

        public StaffService(ApplicationDbContext context, ITicketRepository ticketRepository, ILogger<StaffService> logger)
        {
            _context = context;
            _ticketRepository = ticketRepository;
            _logger = logger;
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

               
                var existingEmail = await _context.Staffs.AnyAsync(s => s.Email == dto.Email);
                if (existingEmail)
                    return Result<Staff>.Failure("Bu email adresi zaten kullanılıyor.");

                var staff = new Staff
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Phone = dto.Phone,
                    Department = dto.Department,
                    MaxConcurrentTickets = dto.MaxConcurrentTickets,
                    IsActive = true
                };

                await _context.Staffs.AddAsync(staff);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Yeni çalışan oluşturuldu: {StaffName}", staff.FullName);
                return Result<Staff>.Success(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Çalışan oluşturulurken hata");
                return Result<Staff>.Failure("Çalışan oluşturulurken bir hata oluştu.");
            }
        }

        public async Task<Result> UpdateStaffAsync(Guid staffId, UpdateStaffDto dto)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                if (!string.IsNullOrWhiteSpace(dto.FullName) && dto.FullName != "string")
                    staff.FullName = dto.FullName;

                if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != "string")
                {
                    var emailExists = await _context.Staffs.AnyAsync(s => s.Email == dto.Email && s.Id != staffId);
                    if (emailExists)
                        return Result.Failure("Bu email adresi başka bir çalışan tarafından kullanılıyor.");
                    staff.Email = dto.Email;
                }

                if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != "string")
                    staff.Phone = dto.Phone;

                if (!string.IsNullOrWhiteSpace(dto.Department) && dto.Department != "string")
                    staff.Department = dto.Department;

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

        public async Task<Result> DeleteStaffAsync(Guid staffId)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("�al��an bulunamad�.");

                // Atanm�� a��k biletleri kontrol et
                var assignedTickets = await _context.Tickets
                    .CountAsync(t => t.AssignedPerson == staff.FullName && t.Status != TicketStatus.Resolved);

                if (assignedTickets > 0)
                    return Result.Failure($"Bu �al��an�n {assignedTickets} adet a��k ticket'� var. �nce ticket'lar� ba�ka birine atay�n.");

                _context.Staffs.Remove(staff);
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
            var query = _context.Staffs.AsQueryable();

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
                    Department = s.Department,
                    IsActive = s.IsActive,
                    MaxConcurrentTickets = s.MaxConcurrentTickets,
                    CreatedDate = s.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<StaffDto?> GetStaffByIdAsync(Guid staffId)
        {
            return await _context.Staffs
                .Where(s => s.Id == staffId)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Department = s.Department,
                    IsActive = s.IsActive,
                    MaxConcurrentTickets = s.MaxConcurrentTickets,
                    CreatedDate = s.CreatedDate
                })
                .FirstOrDefaultAsync();
        }

       

        public async Task<List<StaffWorkloadDto>> GetStaffWorkloadsAsync()
        {
            var staffList = await _context.Staffs.Where(s => s.IsActive).ToListAsync();
            var tickets = await _context.Tickets.ToListAsync();
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);

            var workloads = staffList.Select(staff =>
            {
                var staffTickets = tickets.Where(t => t.AssignedPerson == staff.FullName).ToList();
                var openTickets = staffTickets.Count(t => t.Status == TicketStatus.Open);
                var processingTickets = staffTickets.Count(t => t.Status == TicketStatus.Processing);
                var activeTickets = openTickets + processingTickets;

                return new StaffWorkloadDto
                {
                    Id = staff.Id,
                    FullName = staff.FullName,
                    Department = staff.Department,
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

        

        public async Task<Result> AssignTicketToStaffAsync(Guid ticketId, Guid staffId, string? note = null)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("�al��an bulunamad�.");

                if (!staff.IsActive)
                    return Result.Failure("Bu �al��an aktif de�il.");

                
                var currentTickets = await _context.Tickets
                    .CountAsync(t => t.AssignedPerson == staff.FullName && 
                                    (t.Status == TicketStatus.Open || t.Status == TicketStatus.Processing));

                if (currentTickets >= staff.MaxConcurrentTickets)
                    return Result.Failure($"{staff.FullName} maksimum ticket kapasitesine ula�m�� ({currentTickets}/{staff.MaxConcurrentTickets}).");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamad�.");

                var oldAssignee = ticket.AssignedPerson;
                ticket.AssignedPerson = staff.FullName;
                ticket.Status = TicketStatus.Processing;
                ticket.UpdatedDate = DateTime.UtcNow;

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
                    CreatedDate = DateTime.UtcNow
                });

                _logger.LogInformation("Ticket atand�: {TicketId} ? {StaffName}", ticketId, staff.FullName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket atan�rken hata: {TicketId}", ticketId);
                return Result.Failure("Ticket atan�rken bir hata olu�tu.");
            }
        }

        public async Task<Result<int>> BulkAssignTicketsAsync(List<Guid> ticketIds, Guid staffId)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(staffId);
                if (staff == null)
                    return Result<int>.Failure("�al��an bulunamad�.");

                if (!staff.IsActive)
                    return Result<int>.Failure("Bu �al��an aktif de�il.");

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
                _logger.LogError(ex, "Toplu atama hatas�");
                return Result<int>.Failure("Toplu atama s�ras�nda hata olu�tu.");
            }
        }

        public async Task<Result<AutoAssignResultDto>> AutoAssignTicketAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result<AutoAssignResultDto>.Failure("Ticket bulunamad�.");

                if (!string.IsNullOrEmpty(ticket.AssignedPerson))
                    return Result<AutoAssignResultDto>.Failure("Bu ticket zaten birine atanm��.");

                
                var workloads = await GetStaffWorkloadsAsync();
                var availableStaff = workloads
                    .Where(w => w.IsAvailable && w.IsActive)
                    .OrderBy(w => w.WorkloadPercentage)
                    .FirstOrDefault();

                if (availableStaff == null)
                    return Result<AutoAssignResultDto>.Failure("M�sait �al��an bulunamad�.");

                var assignResult = await AssignTicketToStaffAsync(ticketId, availableStaff.Id, "Otomatik atama");
                if (!assignResult.IsSuccess)
                    return Result<AutoAssignResultDto>.Failure(assignResult.ErrorMessage!);

                return Result<AutoAssignResultDto>.Success(new AutoAssignResultDto
                {
                    Success = true,
                    Message = $"Ticket otomatik olarak atand�",
                    AssignedTo = availableStaff.FullName,
                    StaffId = availableStaff.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik atama hatas�: {TicketId}", ticketId);
                return Result<AutoAssignResultDto>.Failure("Otomatik atama s�ras�nda hata olu�tu.");
            }
        }

        

        public async Task<Result> UnassignTicketAsync(Guid ticketId)
        {
            try
            {
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamad�.");

                var oldAssignee = ticket.AssignedPerson;
                if (string.IsNullOrEmpty(oldAssignee))
                    return Result.Failure("Bu ticket zaten kimseye atanmam��.");

                ticket.AssignedPerson = null;
                ticket.Status = TicketStatus.Open;
                ticket.UpdatedDate = DateTime.UtcNow;

                await _ticketRepository.UpdateAsync(ticket);

                await _ticketRepository.AddHistoryAsync(new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = "Admin",
                    Description = $"Ticket '{oldAssignee}' personelinden geri alındı",
                    CreatedDate = DateTime.UtcNow
                });

                _logger.LogInformation("Ticket ataması kaldırıldı: {TicketId}", ticketId);
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
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == email);
            if (staff == null || !staff.IsActive)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(password, staff.PasswordHash))
                return null;

            return staff;
        }

        // Admin şifre sıfırlama
        public async Task<Result> ResetStaffPasswordAsync(Guid staffId, string newPassword)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(staffId);
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
        public async Task<List<TicketDto>> GetMyTicketsAsync(Guid staffId)
        {
            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff == null)
                return new List<TicketDto>();

            var tickets = await _context.Tickets
                .Include(t => t.Tenant)
                .Include(t => t.Asset)
                .Where(t => t.AssignedPerson == staff.FullName)
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssignedPerson = t.AssignedPerson,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    TenantId = t.TenantId,
                    CompanyName = t.Tenant != null ? t.Tenant.CompanyName : null,
                    AssetId = t.AssetId,
                    AssetName = t.Asset != null ? t.Asset.ProductName : null,
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
                .Include(t => t.Asset)
                .Where(t => string.IsNullOrEmpty(t.AssignedPerson) && t.Status == TicketStatus.Open)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.CreatedDate)
                .Select(t => new TicketDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssignedPerson = t.AssignedPerson,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    TenantId = t.TenantId,
                    CompanyName = t.Tenant != null ? t.Tenant.CompanyName : null,
                    AssetId = t.AssetId,
                    AssetName = t.Asset != null ? t.Asset.ProductName : null,
                    ImagePath = t.ImagePath
                })
                .ToListAsync();

            return tickets;
        }

        // Staff ticket'ı kendine atasın
        public async Task<Result> ClaimTicketAsync(Guid ticketId, Guid staffId)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                if (!staff.IsActive)
                    return Result.Failure("Hesabınız aktif değil.");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı.");

                if (!string.IsNullOrEmpty(ticket.AssignedPerson))
                    return Result.Failure("Bu ticket zaten birine atanmış.");

                // Kapasite kontrolü
                var currentTickets = await _context.Tickets
                    .CountAsync(t => t.AssignedPerson == staff.FullName && 
                                    (t.Status == TicketStatus.Open || t.Status == TicketStatus.Processing));

                if (currentTickets >= staff.MaxConcurrentTickets)
                    return Result.Failure($"Maksimum ticket kapasitesine ulaştınız ({currentTickets}/{staff.MaxConcurrentTickets}).");

                ticket.AssignedPerson = staff.FullName;
                ticket.Status = TicketStatus.Processing;
                ticket.UpdatedDate = DateTime.UtcNow;

                await _ticketRepository.UpdateAsync(ticket);

                await _ticketRepository.AddHistoryAsync(new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = staff.FullName,
                    Description = $"Ticket '{staff.FullName}' tarafından alındı",
                    CreatedDate = DateTime.UtcNow
                });

                _logger.LogInformation("Ticket alındı: {TicketId} -> {StaffName}", ticketId, staff.FullName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket alınırken hata: {TicketId}", ticketId);
                return Result.Failure("Ticket alınırken bir hata oluştu.");
            }
        }

        // Staff ticket'ı bıraksın (unassign from self)
        public async Task<Result> ReleaseTicketAsync(Guid ticketId, Guid staffId)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(staffId);
                if (staff == null)
                    return Result.Failure("Çalışan bulunamadı.");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                    return Result.Failure("Ticket bulunamadı.");

                if (ticket.AssignedPerson != staff.FullName)
                    return Result.Failure("Bu ticket size atanmamış.");

                ticket.AssignedPerson = null;
                ticket.Status = TicketStatus.Open;
                ticket.UpdatedDate = DateTime.UtcNow;

                await _ticketRepository.UpdateAsync(ticket);

                await _ticketRepository.AddHistoryAsync(new TicketHistory
                {
                    TicketId = ticketId,
                    ActionBy = staff.FullName,
                    Description = $"Ticket '{staff.FullName}' tarafından bırakıldı",
                    CreatedDate = DateTime.UtcNow
                });

                _logger.LogInformation("Ticket bırakıldı: {TicketId} <- {StaffName}", ticketId, staff.FullName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ticket bırakılırken hata: {TicketId}", ticketId);
                return Result.Failure("Ticket bırakılırken bir hata oluştu.");
            }
        }

        // Staff profilini getir
        public async Task<StaffDto?> GetStaffProfileAsync(Guid staffId)
        {
            return await _context.Staffs
                .Where(s => s.Id == staffId)
                .Select(s => new StaffDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Department = s.Department,
                    IsActive = s.IsActive,
                    MaxConcurrentTickets = s.MaxConcurrentTickets,
                    CreatedDate = s.CreatedDate
                })
                .FirstOrDefaultAsync();
        }

        // Staff workload özeti
        public async Task<StaffWorkloadDto?> GetMyWorkloadAsync(Guid staffId)
        {
            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff == null)
                return null;

            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);

            var staffTickets = await _context.Tickets
                .Where(t => t.AssignedPerson == staff.FullName)
                .ToListAsync();

            var openTickets = staffTickets.Count(t => t.Status == TicketStatus.Open);
            var processingTickets = staffTickets.Count(t => t.Status == TicketStatus.Processing);
            var activeTickets = openTickets + processingTickets;

            return new StaffWorkloadDto
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Department = staff.Department,
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
    }
}
