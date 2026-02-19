namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface ILookupService
    {
        // Generic: get parameters by Group from SystemParameter table
        Task<List<KobiMuhendislikTicket.Domain.Entities.System.SystemParameter>> GetByGroupAsync(string group);
    }
}
