using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Request;

public class EntityRoleMappingRequest
{
    [Required]
    public int EntityTypeId { get; set; }
    [Required]
    public string RoleId { get; set; }
    public int Status { get; set; }
}
