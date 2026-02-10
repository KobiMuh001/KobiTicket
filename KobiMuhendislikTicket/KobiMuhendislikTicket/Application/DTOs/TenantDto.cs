namespace KobiMuhendislikTicket.Application.DTOs
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public required string CompanyName { get; set; }
        public required string TaxNumber { get; set; }
        public required string Email { get; set; }
    }

    public class CreateTenantDto
    {
        public required string CompanyName { get; set; }
        public required string TaxNumber { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string PhoneNumber { get; set; }
    }

    public class UpdateTenantDto
    {
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    

    
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

   
    public class AdminResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    
    public class DeleteTenantResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedAssetsCount { get; set; }
        public int DeletedTicketsCount { get; set; }
    }
}