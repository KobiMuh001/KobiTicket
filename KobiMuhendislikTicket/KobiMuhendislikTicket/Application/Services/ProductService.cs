using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Common;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using KobiMuhendislikTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KobiMuhendislikTicket.Application.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductListItemDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .Select(p => new ProductListItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    TenantCount = p.ProductTenants.Count
                })
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateProductAsync(CreateProductDto dto)
        {
            var productName = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(productName))
                return (false, "Ürün adı zorunludur.");

            var exists = await _context.Products
                .AnyAsync(p => !p.IsDeleted && p.Name.ToLower() == productName.ToLower());

            if (exists)
                return (false, "Bu ürün adı zaten kayıtlı.");

            await _context.Products.AddAsync(new Product
            {
                Name = productName,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
            });

            await _context.SaveChangesAsync();
            return (true, "Ürün başarıyla eklendi.");
        }

        public async Task<(bool Success, string Message)> UpdateProductAsync(int productId, UpdateProductDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
            if (product == null)
                return (false, "Ürün bulunamadı.");

            var productName = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(productName))
                return (false, "Ürün adı zorunludur.");

            var exists = await _context.Products
                .AnyAsync(p => !p.IsDeleted && p.Id != productId && p.Name.ToLower() == productName.ToLower());

            if (exists)
                return (false, "Bu ürün adı zaten kayıtlı.");

            product.Name = productName;
            product.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

            await _context.SaveChangesAsync();
            return (true, "Ürün başarıyla güncellendi.");
        }

        public async Task<(bool Success, string Message)> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
            if (product == null)
                return (false, "Ürün bulunamadı.");

            product.IsDeleted = true;

            var relations = await _context.ProductTenants
                .Where(pt => pt.ProductId == productId)
                .ToListAsync();

            if (relations.Count > 0)
                _context.ProductTenants.RemoveRange(relations);

            await _context.SaveChangesAsync();
            return (true, "Ürün başarıyla silindi.");
        }

        public async Task<ProductTenantsDto?> GetProductTenantsAsync(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Select(p => new ProductTenantsDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Description = p.Description,
                    Tenants = p.ProductTenants
                        .Where(pt => pt.Tenant != null && !pt.Tenant.IsDeleted)
                        .Select(pt => new ProductTenantItemDto
                        {
                            TenantId = pt.TenantId,
                            CompanyName = pt.Tenant!.CompanyName,
                            Email = pt.Tenant.Email,
                            Username = pt.Tenant.Username,
                            WarrantyEndDate = pt.WarrantyEndDate,
                            AcquisitionDate = pt.AcquisitionDate
                        })
                        .OrderBy(t => t.CompanyName)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return product;
        }

        public async Task<(bool Success, string Message)> AssignProductToTenantAsync(int productId, int tenantId, AssignProductTenantDto dto)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId && !p.IsDeleted);
            if (!productExists)
                return (false, "Ürün bulunamadı.");

            var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == tenantId && !t.IsDeleted);
            if (!tenantExists)
                return (false, "Firma bulunamadı.");

            var relationExists = await _context.ProductTenants.AnyAsync(pt => pt.ProductId == productId && pt.TenantId == tenantId);
            if (relationExists)
                return (false, "Bu ürün zaten ilgili firmaya atanmış.");

            if (dto.WarrantyEndDate == default)
                return (false, "Garanti bitiş tarihi zorunludur.");

            var acquisitionDate = dto.AcquisitionDate ?? DateTimeHelper.GetLocalNow();
            if (dto.WarrantyEndDate < acquisitionDate)
                return (false, "Garanti bitiş tarihi sahiplik tarihinden önce olamaz.");

            await _context.ProductTenants.AddAsync(new ProductTenant
            {
                ProductId = productId,
                TenantId = tenantId,
                WarrantyEndDate = dto.WarrantyEndDate,
                AcquisitionDate = acquisitionDate
            });

            await _context.SaveChangesAsync();
            return (true, "Ürün firmaya başarıyla atandı.");
        }

        public async Task<(bool Success, string Message)> UpdateProductTenantAsync(int productId, int tenantId, UpdateProductTenantDto dto)
        {
            if (dto.WarrantyEndDate == default)
                return (false, "Garanti bitiş tarihi zorunludur.");

            var relation = await _context.ProductTenants
                .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TenantId == tenantId);

            if (relation == null)
                return (false, "Ürün-firma ilişkisi bulunamadı.");

            var acquisitionDate = dto.AcquisitionDate ?? relation.AcquisitionDate ?? DateTimeHelper.GetLocalNow();
            if (dto.WarrantyEndDate < acquisitionDate)
                return (false, "Garanti bitiş tarihi sahiplik tarihinden önce olamaz.");

            relation.WarrantyEndDate = dto.WarrantyEndDate;
            relation.AcquisitionDate = acquisitionDate;

            await _context.SaveChangesAsync();
            return (true, "Ürün-firma bilgileri başarıyla güncellendi.");
        }

        public async Task<(bool Success, string Message)> RemoveProductFromTenantAsync(int productId, int tenantId)
        {
            var relation = await _context.ProductTenants
                .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TenantId == tenantId);

            if (relation == null)
                return (false, "Ürün-firma ilişkisi bulunamadı.");

            _context.ProductTenants.Remove(relation);
            await _context.SaveChangesAsync();

            return (true, "Ürün firma ilişkisinden kaldırıldı.");
        }

        public async Task<List<TenantProductItemDto>> GetTenantProductsAsync(int tenantId)
        {
            return await _context.ProductTenants
                .AsNoTracking()
                .Where(pt => pt.TenantId == tenantId && !pt.Product.IsDeleted)
                .Select(pt => new TenantProductItemDto
                {
                    ProductId = pt.ProductId,
                    ProductName = pt.Product.Name,
                    Description = pt.Product.Description,
                    WarrantyEndDate = pt.WarrantyEndDate,
                    AcquisitionDate = pt.AcquisitionDate
                })
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }
    }
}
