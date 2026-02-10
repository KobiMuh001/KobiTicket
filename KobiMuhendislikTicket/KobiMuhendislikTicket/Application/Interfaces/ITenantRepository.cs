using KobiMuhendislikTicket.Domain.Entities;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id);
        Task<Tenant?> GetByTaxNumberAsync(string taxNumber);
        Task<List<Tenant>> GetAllAsync();
        Task AddAsync(Tenant tenant);
        Task UpdateAsync(Tenant tenant);
        Task DeleteAsync(Guid id);
    }
}