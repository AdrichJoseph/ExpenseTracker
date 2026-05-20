using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTracker.Infrastructure.Auth;

// Builds and signs JWTs. A JWT is three base64 chunks: header.payload.signature.
// The payload holds "claims" (id, email, role). The signature proves we issued it.
public class JwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config) => _config = config;

    public (string token, DateTime expiresAt) Generate(User user, string role)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"]!;
        var issuer = jwt["Issuer"] ?? "ExpenseTracker";
        var audience = jwt["Audience"] ?? "ExpenseTracker";
        var hours = int.TryParse(jwt["ExpiryHours"], out var h) ? h : 8;

        var expiresAt = DateTime.UtcNow.AddHours(hours);

        // Claims = the facts baked into the token. ASP.NET reads these on every request.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, role),               // <-- this is what [Authorize(Roles=...)] checks
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Sign with HMAC-SHA256 using our secret key.
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}