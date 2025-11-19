using Microsoft.IdentityModel.Tokens;

namespace UserService.Helper;

public class JwtIssuerOptions
{
    public string? JwtTokenString { get; set; }
    public string? Issuer { get; set; }
    public string? Subject { get; set; }
    public string? Audience { get; set; }
    public DateTime NotBefore => DateTime.UtcNow;
    public DateTime IssuedAt => DateTime.UtcNow;
    public int ValidFor { get; set; } //= TimeSpan.FromMinutes(5);   
    public DateTime Expiration => IssuedAt.AddDays(ValidFor);
    public Func<Task<string>> JtiGenerator =>  () => Task.FromResult(Guid.NewGuid().ToString());
    public SigningCredentials? SigningCredentials { get; set; }
    public string? IssuerSigningKey { get; set; }
    public string? ValidIssuer { get; set; }
    public string? ValidAudience { get; set; }
}