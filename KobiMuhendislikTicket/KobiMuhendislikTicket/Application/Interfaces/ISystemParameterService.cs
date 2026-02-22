using KobiMuhendislikTicket.Application.DTOs;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface ISystemParameterService
    {
        Task<List<SystemParameterDto>> GetByGroupAsync(string group);
        Task<List<string>> GetGroupsAsync();
        Task<SystemParameterDto?> GetByIdAsync(int id);
        Task<SystemParameterDto?> GetByGroupAndKeyAsync(string group, int numericKey);
        Task<(bool Success, string Message, int? Id)> CreateAsync(CreateSystemParameterDto dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, UpdateSystemParameterDto dto);
        Task<(bool Success, string Message)> UpdateByGroupAndKeyAsync(string group, int numericKey, UpdateSystemParameterDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<(bool Success, string Message)> DeleteByGroupAndKeyAsync(string group, int numericKey);
        Task<(bool Success, string Message)> ReorderGroupAsync(string group, List<int> orderedNumericKeys);
    }
}
