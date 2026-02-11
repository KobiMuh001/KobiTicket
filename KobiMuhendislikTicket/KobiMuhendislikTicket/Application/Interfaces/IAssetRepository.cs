using KobiMuhendislikTicket.Domain.Entities;
using System.Threading.Tasks;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface IAssetRepository
    {
        Task<List<Asset>> GetByTenantIdAsync(int tenantId);
        Task<List<Asset>> GetAllAsync();
        Task<Asset?> GetByIdAsync(int id);
        Task<Asset?> GetBySerialNumberAsync(string serialNumber);
        Task AddAsync(Asset asset);
        Task UpdateAsync(Asset asset);
        Task DeleteAsync(int id);
    }
}