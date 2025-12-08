using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Request;

public class LoginRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
}
