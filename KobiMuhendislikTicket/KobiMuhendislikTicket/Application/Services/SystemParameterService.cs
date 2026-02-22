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
            // Prefer numeric key (NumericKey); fallback to parsing string Key if present.
            int? keyInt = p.NumericKey;
            if (!keyInt.HasValue && !string.IsNullOrWhiteSpace(p.Key))
            {
                if (int.TryParse(p.Key, out var parsed)) keyInt = parsed;
            }

            return new SystemParameterDto
            {
                Group = p.Group,
                Key = keyInt,
                NumericKey = p.NumericKey.HasValue ? p.NumericKey : (int.TryParse(p.Key, out var v) ? v : (int?)null),
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
                .ThenBy(x => x.NumericKey)
                .ToListAsync();

            return list.Select(p => new SystemParameterDto
            {
                Group = p.Group,
                // map numeric key with fallback to parsing string Key
                Key = p.NumericKey.HasValue ? p.NumericKey : (int.TryParse(p.Key, out var v) ? v : (int?)null),
                NumericKey = p.NumericKey.HasValue ? p.NumericKey : (int.TryParse(p.Key, out var v2) ? v2 : (int?)null),
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
            // determine numeric key to use: if dto.Key provided use it, otherwise pick max(NumericKey)+1 for group
            int keyToUse;
            if (dto.Key.HasValue)
            {
                keyToUse = dto.Key.Value;
            }
            else
            {
                var maxKey = await _db.SystemParameters
                    .Where(x => x.Group == dto.Group)
                    .Select(x => (int?)x.NumericKey)
                    .MaxAsync() ?? 0;
                keyToUse = maxKey + 1;
            }

            // ensure unique Key within group (check numeric and string forms)
            var keyAsString = keyToUse.ToString();
            var exists = await _db.SystemParameters.AnyAsync(p => p.Group == dto.Group && (p.NumericKey == keyToUse || p.Key == keyAsString));
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
                // store numeric key and keep string Key for backward compatibility
                NumericKey = keyToUse,
                Key = keyToUse.ToString(),
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
            if (dto.SortOrder.HasValue) p.SortOrder = dto.SortOrder.Value;
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

        public async Task<SystemParameterDto?> GetByGroupAndKeyAsync(string group, int numericKey)
        {
            var p = await _db.SystemParameters
                .Where(x => x.Group == group && (x.NumericKey == numericKey || x.Key == numericKey.ToString()))
                .FirstOrDefaultAsync();
            if (p == null) return null;
            int? keyInt = p.NumericKey;
            if (!keyInt.HasValue && !string.IsNullOrWhiteSpace(p.Key) && int.TryParse(p.Key, out var parsed)) keyInt = parsed;
            return new SystemParameterDto
            {
                Group = p.Group,
                Key = keyInt,
                NumericKey = p.NumericKey.HasValue ? p.NumericKey : (int.TryParse(p.Key, out var v) ? v : (int?)null),
                Value = p.Value,
                Value2 = p.Value2,
                Description = p.Description,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                DataType = p.DataType,
                CreatedDate = p.CreatedDate
            };
        }

        public async Task<(bool Success, string Message)> UpdateByGroupAndKeyAsync(string group, int numericKey, UpdateSystemParameterDto dto)
        {
            var p = await _db.SystemParameters
                .Where(x => x.Group == group && (x.NumericKey == numericKey || x.Key == numericKey.ToString()))
                .FirstOrDefaultAsync();
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
            if (dto.SortOrder.HasValue) p.SortOrder = dto.SortOrder.Value;
            await _db.SaveChangesAsync();
            return (true, "Güncellendi.");
        }

        public async Task<(bool Success, string Message)> DeleteByGroupAndKeyAsync(string group, int numericKey)
        {
            var p = await _db.SystemParameters
                .Where(x => x.Group == group && (x.NumericKey == numericKey || x.Key == numericKey.ToString()))
                .FirstOrDefaultAsync();
            if (p == null) return (false, "Bulunamadı.");
            _db.SystemParameters.Remove(p);
            await _db.SaveChangesAsync();
            return (true, "Silindi.");
        }

        public async Task<(bool Success, string Message)> ReorderGroupAsync(string group, List<int> orderedNumericKeys)
        {
            // Fetch all parameters for the group
            var items = await _db.SystemParameters
                .Where(x => x.Group == group)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.NumericKey)
                .ToListAsync();

            if (items == null || items.Count == 0) return (false, "Grup bulunamadı veya boş.");

            // Map numericKey -> entity for quick lookup
            var map = items.ToDictionary(p => p.NumericKey ?? (int?)null, p => p);

            // Assign SortOrder for provided keys in the given sequence
            int order = 1;
            var handled = new HashSet<int>();
            foreach (var key in orderedNumericKeys)
            {
                var entity = items.FirstOrDefault(p => (p.NumericKey.HasValue && p.NumericKey.Value == key) || p.Key == key.ToString());
                if (entity != null)
                {
                    entity.SortOrder = order++;
                    if (entity.NumericKey.HasValue) handled.Add(entity.NumericKey.Value);
                }
            }

            // For any remaining items not included in orderedNumericKeys, append them preserving previous relative order
            foreach (var p in items.Where(p => !(p.NumericKey.HasValue && handled.Contains(p.NumericKey.Value))))
            {
                p.SortOrder = order++;
            }

            await _db.SaveChangesAsync();
            return (true, "Sıralama güncellendi.");
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
