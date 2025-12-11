using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Interface;
using UserService.Models.Helpers;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;
    private readonly IConfiguration _config;

    public TokenService(IOptions<JwtSettings> jwt, IConfiguration config)
    {
        _jwt = jwt.Value;
        _config = config;
    }

    public string GenerateToken(string fullName, int roleId, int bankId, int districtId, string dfp)
    {
        var claims = new[]
            {
            new Claim("Name", fullName),
            new Claim("RoleId", roleId.ToString()),
            new Claim("BankId", bankId.ToString()),
            new Claim("DistrictId", districtId.ToString()),
            new Claim("dfp", dfp)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:secretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToInt64(_config["Jwt:ExpiryInMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
