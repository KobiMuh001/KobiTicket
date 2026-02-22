namespace KobiMuhendislikTicket.Application.DTOs
{
    public class SystemParameterDto
    {
        public string Group { get; set; } = string.Empty;
        // Expose both the legacy numeric key and the new NumericKey column.
        // Clients should prefer `NumericKey` for business logic.
        public int? Key { get; set; }
        public int? NumericKey { get; set; }
        public string? Value { get; set; }
        public string? Value2 { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string DataType { get; set; } = "String";
        public DateTime CreatedDate { get; set; }
    }

    public class CreateSystemParameterDto
    {
        public string Group { get; set; } = string.Empty;
        // If `Key` is not provided, service will assign next numeric key for the group.
        public int? Key { get; set; }
        public string? Value { get; set; }
        public string? Value2 { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string DataType { get; set; } = "String";
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateSystemParameterDto
    {
        public string? Value { get; set; }
        public string? Value2 { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public string? DataType { get; set; }
        public int? SortOrder { get; set; }
    }
}
