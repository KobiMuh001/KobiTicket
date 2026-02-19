namespace KobiMuhendislikTicket.Application.DTOs
{
    public class SystemParameterDto
    {
        public int Id { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
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
        public string Key { get; set; } = string.Empty;
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
    }
}
