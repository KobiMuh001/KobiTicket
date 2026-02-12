using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Domain.Entities;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace KobiMuhendislikTicket.Application.Services
{
    public class TenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ApplicationDbContext _context;

        public TenantService(ITenantRepository tenantRepository, ApplicationDbContext context)
        {
            _tenantRepository = tenantRepository;
            _context = context;
        }

        public async Task<string> CreateTenantAsync(CreateTenantDto dto)
        {
            var existing = await _tenantRepository.GetByTaxNumberAsync(dto.TaxNumber);
            if (existing != null) return "Bu Vergi Numarası zaten sistemde kayıtlı.";

            // Email kontrolü (case-insensitive)
            var existingEmail = await _context.Tenants.FirstOrDefaultAsync(t => t.Email.ToLower() == dto.Email.ToLower());
            if (existingEmail != null) return "Bu email adresi zaten sistemde kayıtlı.";

            var tenant = new Tenant
            {
                CompanyName = dto.CompanyName,
                TaxNumber = dto.TaxNumber,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber
            };

            await _tenantRepository.AddAsync(tenant);
            return "Ok";
        }

        

        public async Task<Result> UpdateTenantAsync(int tenantId, UpdateTenantDto dto)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result.Failure("Müşteri bulunamadı.");

            
            if (IsValidValue(dto.CompanyName))
                tenant.CompanyName = dto.CompanyName!;

            if (IsValidValue(dto.Email))
            {
                // Email zaten başka bir müşteri tarafından kullanılıyor mu kontrol et
                var existingEmail = await _context.Tenants.FirstOrDefaultAsync(t => t.Email.ToLower() == (dto.Email ?? "").ToLower() && t.Id != tenantId);
                if (existingEmail != null)
                    return Result.Failure("Bu email adresi başka bir müşteri tarafından zaten kullanılmaktadır.");
                
                tenant.Email = dto.Email!;
            }

            if (IsValidValue(dto.PhoneNumber))
                tenant.PhoneNumber = dto.PhoneNumber!;

            tenant.UpdatedDate = DateTimeHelper.GetLocalNow();

            await _tenantRepository.UpdateAsync(tenant);
            return Result.Success();
        }

        
        private static bool IsValidValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            
            
            if (value.Equals("string", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        

        public async Task<Result> ChangePasswordAsync(int tenantId, ChangePasswordDto dto)
        {
            
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return Result.Failure("Mevcut şifre gereklidir.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return Result.Failure("Yeni şifre gereklidir.");

            if (dto.NewPassword.Length < 6)
                return Result.Failure("Yeni şifre en az 6 karakter olmalıdır.");

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return Result.Failure("Yeni şifreler eşleşmiyor.");

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result.Failure("Müşteri bulunamadı.");

            
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, tenant.PasswordHash))
                return Result.Failure("Mevcut şifre hatalı.");

            
            tenant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            tenant.UpdatedDate = DateTimeHelper.GetLocalNow();

            await _tenantRepository.UpdateAsync(tenant);
            return Result.Success();
        }

        

        public async Task<Result> AdminResetPasswordAsync(int tenantId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return Result.Failure("Yeni şifre gereklidir.");

            if (newPassword.Length < 6)
                return Result.Failure("Şifre en az 6 karakter olmalıdır.");

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result.Failure("Müşteri bulunamadı.");

            tenant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            tenant.UpdatedDate = DateTimeHelper.GetLocalNow();

            await _tenantRepository.UpdateAsync(tenant);
            return Result.Success();
        }

        

        public async Task<Result<DeleteTenantResultDto>> DeleteTenantAsync(int tenantId, bool forceDelete = false)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result<DeleteTenantResultDto>.Failure("Müşteri bulunamadı.");

        
            var assetCount = await _context.Assets.CountAsync(a => a.TenantId == tenantId);
            var ticketCount = await _context.Tickets.CountAsync(t => t.TenantId == tenantId);
            var openTicketCount = await _context.Tickets.CountAsync(t => t.TenantId == tenantId && t.Status != Domain.Enums.TicketStatus.Resolved);

            
            if (openTicketCount > 0 && !forceDelete)
            {
                return Result<DeleteTenantResultDto>.Failure(
                    $"Bu müşterinin {openTicketCount} adet açık ticket'ı var. Silmek için forceDelete=true kullanın.");
            }

            
            var ticketIds = await _context.Tickets.Where(t => t.TenantId == tenantId).Select(t => t.Id).ToListAsync();
            var comments = await _context.TicketComments.Where(c => ticketIds.Contains(c.TicketId)).ToListAsync();
            _context.TicketComments.RemoveRange(comments);

           
            var histories = await _context.TicketHistories.Where(h => ticketIds.Contains(h.TicketId)).ToListAsync();
            _context.TicketHistories.RemoveRange(histories);

            
            var tickets = await _context.Tickets.Where(t => t.TenantId == tenantId).ToListAsync();
            _context.Tickets.RemoveRange(tickets);

            
            var assets = await _context.Assets.Where(a => a.TenantId == tenantId).ToListAsync();
            _context.Assets.RemoveRange(assets);

            
            await _tenantRepository.DeleteAsync(tenantId);

            await _context.SaveChangesAsync();

            return Result<DeleteTenantResultDto>.Success(new DeleteTenantResultDto
            {
                Success = true,
                Message = $"Müşteri ve ilişkili tüm veriler başarıyla silindi.",
                DeletedAssetsCount = assetCount,
                DeletedTicketsCount = ticketCount
            });
        }

        
        public async Task<Tenant?> GetByIdAsync(int tenantId)
        {
            return await _tenantRepository.GetByIdAsync(tenantId);
        }
    }
}