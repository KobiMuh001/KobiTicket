using KobiMuhendislikTicket.Application.Interfaces;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Domain.Entities;

namespace KobiMuhendislikTicket.Application.Services
{
    public class AssetService
    {
        private readonly IAssetRepository _assetRepository;
        public AssetService(IAssetRepository assetRepository) => _assetRepository = assetRepository;

        public async Task<List<Asset>> GetMyAssetsAsync(Guid tenantId)
        {
            return await _assetRepository.GetByTenantIdAsync(tenantId);
        }

        public async Task<List<AssetListItemDto>> GetAllAssetsAsync()
        {
            var assets = await _assetRepository.GetAllAsync();
            var now = DateTime.Now;
            return assets.Select(a => new AssetListItemDto
            {
                Id = a.Id,
                ProductName = a.ProductName,
                SerialNumber = a.SerialNumber,
                Status = a.Status,
                WarrantyEndDate = a.WarrantyEndDate,
                IsUnderWarranty = a.WarrantyEndDate > now,
                TenantId = a.TenantId,
                TenantName = a.Tenant?.CompanyName ?? "Bilinmiyor",
                TicketCount = 0,
                CreatedDate = a.CreatedDate
            }).ToList();
        }

        public async Task<AssetDetailDto?> GetAssetByIdAsync(Guid id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null) return null;

            var now = DateTime.Now;
            var daysUntilExpiry = (asset.WarrantyEndDate - now).Days;

            return new AssetDetailDto
            {
                Id = asset.Id,
                ProductName = asset.ProductName,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status,
                WarrantyEndDate = asset.WarrantyEndDate,
                IsUnderWarranty = asset.WarrantyEndDate > now,
                DaysUntilWarrantyExpires = daysUntilExpiry > 0 ? daysUntilExpiry : 0,
                CreatedDate = asset.CreatedDate,
                UpdatedDate = asset.UpdatedDate,
                TenantId = asset.TenantId,
                TenantName = asset.Tenant?.CompanyName ?? "Bilinmiyor",
                TenantEmail = asset.Tenant?.Email ?? "",
                TotalTickets = 0,
                OpenTickets = 0,
                ResolvedTickets = 0
            };
        }

        public async Task<(bool Success, string Message)> CreateAssetAsync(Asset asset)
        {
            if (string.IsNullOrWhiteSpace(asset.SerialNumber))
                return (false, "Seri numarası boş olamaz.");

            // Serial number unique kontrolü
            var existingAsset = await _assetRepository.GetBySerialNumberAsync(asset.SerialNumber);
            if (existingAsset != null)
                return (false, $"Bu seri numarası ({asset.SerialNumber}) zaten sistemde kayıtlı. Lütfen farklı bir seri numarası girin.");

            await _assetRepository.AddAsync(asset);
            return (true, "Varlık başarıyla eklendi.");
        }

        public async Task<(bool Success, string Message)> UpdateAssetAsync(Guid id, UpdateAssetDto dto)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null) 
                return (false, "Varlık bulunamadı.");

            if (string.IsNullOrWhiteSpace(dto.SerialNumber))
                return (false, "Seri numarası boş olamaz.");

            // Seri numarası değiştiriliyorsa, yeni serial number'ın unique olup olmadığını kontrol et
            if (asset.SerialNumber != dto.SerialNumber)
            {
                var existingAsset = await _assetRepository.GetBySerialNumberAsync(dto.SerialNumber);
                if (existingAsset != null)
                    return (false, $"Bu seri numarası ({dto.SerialNumber}) zaten sistemde kayıtlı. Lütfen farklı bir seri numarası girin.");
            }

            asset.ProductName = dto.ProductName;
            asset.SerialNumber = dto.SerialNumber;
            asset.Status = dto.Status;
            asset.WarrantyEndDate = dto.WarrantyEndDate;
            asset.TenantId = dto.TenantId;
            asset.UpdatedDate = DateTime.UtcNow;

            await _assetRepository.UpdateAsync(asset);
            return (true, "Varlık başarıyla güncellendi.");
        }

        public async Task<bool> DeleteAssetAsync(Guid id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null) return false;

            await _assetRepository.DeleteAsync(id);
            return true;
        }
    }
}