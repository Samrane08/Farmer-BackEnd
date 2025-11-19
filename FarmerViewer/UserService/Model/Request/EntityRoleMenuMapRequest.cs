using Repository.Enums;
using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Request;

public class EntityRoleMenuMapRequest
{
    [Required]
    public int EntityRoleId { get; set; }
    [Required]
    public int MenuId { get; set; }
    [Required]
    public Status Status { get; set; }
}
