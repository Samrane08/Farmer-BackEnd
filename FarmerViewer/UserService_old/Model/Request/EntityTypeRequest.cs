using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Request;

public class EntityTypeRequest
{
    [Required]
    public string EntityName { get; set; }
    public int Status { get; set; }
}
