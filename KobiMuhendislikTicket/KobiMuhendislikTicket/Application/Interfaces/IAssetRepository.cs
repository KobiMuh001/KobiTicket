using KobiMuhendislikTicket.Domain.Entities;
using System.Threading.Tasks;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface IAssetRepository
    {
        Task<List<Asset>> GetByTenantIdAsync(Guid tenantId);
        Task<List<Asset>> GetAllAsync();
        Task<Asset?> GetByIdAsync(Guid id);
        Task<Asset?> GetBySerialNumberAsync(string serialNumber);
        Task AddAsync(Asset asset);
        Task UpdateAsync(Asset asset);
        Task DeleteAsync(Guid id);
    }
}