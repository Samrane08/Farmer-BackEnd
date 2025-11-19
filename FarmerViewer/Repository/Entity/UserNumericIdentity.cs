using System.ComponentModel.DataAnnotations;

namespace Repository.Entity;

public class UserNumericIdentity
{
    [Key]
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? LoginAt { get; set; }
    
}
