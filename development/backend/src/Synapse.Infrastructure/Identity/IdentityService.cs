using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Synapse.Application.Common.Interfaces;

namespace Synapse.Infrastructure.Identity;

/// <summary>
/// JWTトークンを生成する認証サービスの実装。
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public IdentityService(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Token)> LoginAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return (false, string.Empty);

        var token = GenerateJwtToken(user);
        return (true, token);
    }

    private string GenerateJwtToken(AppUser user)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey が設定されていません");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "480");

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
