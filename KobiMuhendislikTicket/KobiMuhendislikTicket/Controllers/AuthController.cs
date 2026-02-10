using KobiMuhendislikTicket.Application.DTOs;
using KobiMuhendislikTicket.Application.Services;
using KobiMuhendislikTicket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly StaffService _staffService;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    public AuthController(
        IConfiguration configuration, 
        ApplicationDbContext context, 
        StaffService staffService,
        ITokenBlacklistService tokenBlacklistService)
    {
        _configuration = configuration;
        _context = context;
        _staffService = staffService;
        _tokenBlacklistService = tokenBlacklistService;
    }

    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // Admin login - şifre appsettings'den alınıyor
        var adminUsername = _configuration["Admin:Username"] ?? "admin";
        var adminEmail = _configuration["Admin:Email"] ?? "admin@kobimuhendislik.com";
        var adminPassword = _configuration["Admin:Password"] ?? "Admin123!";
        
        if ((request.Identifier == adminUsername || request.Identifier == adminEmail)
            && request.Password == adminPassword)
        {
            var token = GenerateJwtToken(request.Identifier, "Admin");
            return Ok(new LoginResponseDto { Token = token, CompanyName = "Kobi Mühendislik Yönetim" });
        }
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t =>
            t.TaxNumber == request.Identifier || t.Email == request.Identifier);

        
        if (tenant != null && BCrypt.Net.BCrypt.Verify(request.Password, tenant.PasswordHash))
        {
            var token = GenerateJwtToken(tenant.Id.ToString(), "Customer");
            return Ok(new LoginResponseDto
            {
                Token = token,
                CompanyName = tenant.CompanyName 
            });
        }

        return Unauthorized(new { Message = "Giriş bilgileri hatalı!" });
    }

    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("staff/login")]
    public async Task<IActionResult> StaffLogin([FromBody] StaffLoginDto request)
    {
        // Admin login kontrolü
        var adminUsername = _configuration["Admin:Username"] ?? "admin";
        var adminEmail = _configuration["Admin:Email"] ?? "admin@kobimuhendislik.com";
        var adminPassword = _configuration["Admin:Password"] ?? "Admin123!";
        
        if ((request.Email == adminUsername || request.Email == adminEmail)
            && request.Password == adminPassword)
        {
            var adminToken = GenerateStaffJwtToken("admin", "Admin", "Admin");
            return Ok(new 
            { 
                Token = adminToken, 
                StaffId = (Guid?)null,
                FullName = "Sistem Yöneticisi",
                Department = "Yönetim"
            });
        }

        // Staff login
        var staff = await _staffService.ValidateStaffLoginAsync(request.Email, request.Password);
        
        if (staff == null)
            return Unauthorized(new { Message = "Giriş bilgileri hatalı veya hesap aktif değil!" });

        var token = GenerateStaffJwtToken(staff.Id.ToString(), staff.FullName, "Staff");
        return Ok(new 
        { 
            Token = token, 
            StaffId = staff.Id,
            FullName = staff.FullName,
            Department = staff.Department
        });
    }

    private string GenerateJwtToken(string identifier, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identifier),
            new Claim("sub", identifier),
            new Claim("TenantId", identifier),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateStaffJwtToken(string staffId, string fullName, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, staffId),
            new Claim("sub", staffId),
            new Claim("StaffId", staffId),
            new Claim("FullName", fullName),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var expClaim = User.FindFirst("exp")?.Value;
        
        if (string.IsNullOrEmpty(jti))
            return BadRequest(new { Message = "Geçersiz token." });

        // Token'in expire süresini bul
        var expiresAt = DateTime.UtcNow.AddHours(1); // Default 1 saat
        if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expUnix))
        {
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        }

        _tokenBlacklistService.BlacklistToken(jti, expiresAt);
        return Ok(new { Message = "Başarıyla çıkış yapıldı." });
    }
}