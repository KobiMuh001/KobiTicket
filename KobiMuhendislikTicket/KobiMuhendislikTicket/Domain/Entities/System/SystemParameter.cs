using KobiMuhendislikTicket.Domain.Common;

namespace KobiMuhendislikTicket.Domain.Entities.System
{
    public class SystemParameter : BaseEntity
    {
        
        public required string Group { get; set; }
        // New numeric key: will be used for business logic (enum values).
        // Keep the existing string `Key` for migration/backwards compatibility until final switchover.
        public int? NumericKey { get; set; }
        public required string Key { get; set; }
        public string? Value { get; set; }
        public string? Value2 { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string DataType { get; set; } = "String";
        public int SortOrder { get; set; } = 0;
    }
}
