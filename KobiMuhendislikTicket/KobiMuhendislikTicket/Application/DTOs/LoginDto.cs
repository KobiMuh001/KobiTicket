namespace KobiMuhendislikTicket.Application.DTOs
{
    public class LoginRequestDto
    {
        public required string Identifier { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public required string CompanyName { get; set; }
    }
}