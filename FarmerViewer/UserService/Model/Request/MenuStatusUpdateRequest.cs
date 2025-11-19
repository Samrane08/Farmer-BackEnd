using Repository.Enums;
using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Request;

public class MenuStatusUpdateRequest
{
    [Required]
    public int MenuId { get; set; }
    [Required]
    public Status Status { get; set; }
}
