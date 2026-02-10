using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Interfaces;

namespace KobiMuhendislikTicket.Application.Services
{
    public class AuthService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IConfiguration _configuration;

        public AuthService(ITenantRepository tenantRepository, IConfiguration configuration)
        {
            _tenantRepository = tenantRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            
            var tenant = await _tenantRepository.GetByTaxNumberAsync(dto.Identifier);
            if (tenant == null) return null;

            
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, tenant.PasswordHash);
            if (!isPasswordValid) return null;

            
            var token = GenerateJwtToken(tenant);

            return new LoginResponseDto
            {
                Token = token,
                CompanyName = tenant.CompanyName
            };
        }

        private string GenerateJwtToken(Domain.Entities.Tenant tenant)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, tenant.Id.ToString()),
                new Claim("sub", tenant.Id.ToString()),
                new Claim("TenantId", tenant.Id.ToString()),
                new Claim("TaxNumber", tenant.TaxNumber),
                new Claim(ClaimTypes.Name, tenant.CompanyName),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}