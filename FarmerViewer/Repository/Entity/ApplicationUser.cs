
using Microsoft.AspNetCore.Identity;
using Repository.Enums;

namespace Repository.Entity;
public class ApplicationUser : IdentityUser
{
    public string? UserId { get; set; }   
    public string? Password { get; set; }
    public string? Name { get; set; }
    public Status Status { get; set; }
}




