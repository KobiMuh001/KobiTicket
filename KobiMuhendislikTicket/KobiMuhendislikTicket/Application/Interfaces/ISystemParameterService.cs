using KobiMuhendislikTicket.Application.DTOs;

namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface ISystemParameterService
    {
        Task<List<SystemParameterDto>> GetByGroupAsync(string group);
        Task<List<string>> GetGroupsAsync();
        Task<SystemParameterDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int? Id)> CreateAsync(CreateSystemParameterDto dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, UpdateSystemParameterDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}
