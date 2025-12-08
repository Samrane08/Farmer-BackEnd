using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Request
{
    public class ProfileModel
    {  
        [Required]
        [StringLength(10)]
        public string? Mobile { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }
}
