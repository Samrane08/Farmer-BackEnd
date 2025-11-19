using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace UserService.Helper
{
    public interface ITokenHelper
    {
        string GenerateAccessToken(string userid,string userNumericId,string SessionId, string? RoleEntityId,string District,string WorkFlowId,string deptId);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public class TokenHelper : ITokenHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<JwtIssuerOptions> _jwtOptions;

        public TokenHelper(IOptions<JwtIssuerOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }

        public string GenerateAccessToken(string userid, string userNumericId,string SessionId,string? roleName, string District, string WorkFlowId,string deptId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.IssuerSigningKey));

         

            var Userclaims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userid),
            new Claim(ClaimTypes.NameIdentifier, userid),
            new Claim(ClaimTypes.PrimarySid, userNumericId),
            new Claim(ClaimTypes.SerialNumber, SessionId),
            new Claim(ClaimTypes.Actor, !string.IsNullOrEmpty(roleName)?roleName:""),
            new Claim(ClaimTypes.GroupSid, District),
            new Claim(ClaimTypes.Sid, WorkFlowId),
              new Claim(ClaimTypes.PrimaryGroupSid, deptId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwtToken = new JwtSecurityToken(
              issuer: _jwtOptions.Value.ValidIssuer,
              audience: _jwtOptions.Value.ValidAudience,
              notBefore: _jwtOptions.Value.NotBefore,
              expires: _jwtOptions.Value.Expiration,
              claims: Userclaims,
              signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.JwtTokenString));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
