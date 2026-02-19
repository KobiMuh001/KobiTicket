using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Domain.Entities.System;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KobiMuhendislikTicket.Application.Services
{
    public class SystemParameterService : ISystemParameterService
    {
        private readonly ApplicationDbContext _db;

        public SystemParameterService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SystemParameterDto?> GetByIdAsync(int id)
        {
            var p = await _db.SystemParameters.FindAsync(id);
            if (p == null) return null;
            return new SystemParameterDto
            {
                Id = p.Id,
                Group = p.Group,
                Key = p.Key,
                Value = p.Value,
                Value2 = p.Value2,
                Description = p.Description,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                DataType = p.DataType,
                CreatedDate = p.CreatedDate
            };
        }

        public async Task<List<SystemParameterDto>> GetByGroupAsync(string group)
        {
            var list = await _db.SystemParameters
                .Where(x => x.Group == group && x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Key)
                .ToListAsync();

            return list.Select(p => new SystemParameterDto
            {
                Id = p.Id,
                Group = p.Group,
                Key = p.Key,
                Value = p.Value,
                Value2 = p.Value2,
                Description = p.Description,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                DataType = p.DataType,
                CreatedDate = p.CreatedDate
            }).ToList();
        }

        public async Task<(bool Success, string Message, int? Id)> CreateAsync(CreateSystemParameterDto dto)
        {
            // ensure unique Key within group
            var exists = await _db.SystemParameters.AnyAsync(p => p.Group == dto.Group && p.Key == dto.Key);
            if (exists) return (false, "Aynı anahtar zaten mevcut.", null);
            // determine sortOrder: if caller provided >0 use it, otherwise continue from max existing
            var maxSort = await _db.SystemParameters
                .Where(x => x.Group == dto.Group)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync() ?? 0;
            var sortOrderToUse = dto.SortOrder > 0 ? dto.SortOrder : (maxSort + 1);

            // Only allow Value2 (color) for TicketPriority and TicketStatus groups
            var allowedValue2Groups = new[] { "TicketPriority", "TicketStatus" };
            if (!string.IsNullOrWhiteSpace(dto.Value2))
            {
                if (!allowedValue2Groups.Contains(dto.Group))
                    return (false, "Value2 yalnızca TicketPriority veya TicketStatus gruplarında kullanılabilir.", null);
                // validate hex color (#RGB or #RRGGBB)
                var hex = dto.Value2.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(hex, "^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$"))
                    return (false, "Value2 geçerli bir hex renk kodu olmalı (#RRGGBB veya #RGB).", null);
            }

            var p = new SystemParameter
            {
                Group = dto.Group,
                Key = dto.Key,
                Value = dto.Value,
                Value2 = string.IsNullOrWhiteSpace(dto.Value2) ? null : dto.Value2.Trim(),
                Description = dto.Description,
                IsActive = dto.IsActive,
                DataType = dto.DataType,
                SortOrder = sortOrderToUse
            };
            _db.SystemParameters.Add(p);
            await _db.SaveChangesAsync();
            return (true, "Kayıt oluşturuldu.", p.Id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateSystemParameterDto dto)
        {
            var p = await _db.SystemParameters.FindAsync(id);
            if (p == null) return (false, "Bulunamadı.");
            if (dto.Value != null) p.Value = dto.Value;
            if (dto.Value2 != null)
            {
                var allowedValue2Groups = new[] { "TicketPriority", "TicketStatus" };
                if (!allowedValue2Groups.Contains(p.Group))
                    return (false, "Value2 yalnızca TicketPriority veya TicketStatus gruplarında kullanılabilir.");
                var hex = dto.Value2.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(hex, "^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$"))
                    return (false, "Value2 geçerli bir hex renk kodu olmalı (#RRGGBB veya #RGB)." );
                p.Value2 = hex;
            }
            if (dto.Description != null) p.Description = dto.Description;
            if (dto.IsActive.HasValue) p.IsActive = dto.IsActive.Value;
            if (dto.DataType != null) p.DataType = dto.DataType;
            await _db.SaveChangesAsync();
            return (true, "Güncellendi.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var p = await _db.SystemParameters.FindAsync(id);
            if (p == null) return (false, "Bulunamadı.");
            _db.SystemParameters.Remove(p);
            await _db.SaveChangesAsync();
            return (true, "Silindi.");
        }

        public async Task<List<string>> GetGroupsAsync()
        {
            var groups = await _db.SystemParameters
                .Select(x => x.Group)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
            return groups;
        }
    }
}
