using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Request
{
    public class ForgotPassword
    {
        [Required]
        public string UserName { get; set; }
        [Required]
       // [StringLength(6)]
        public string OTP { get; set; }
        [Required]
       // [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
        public string? IP { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string UserName { get; set; }
        public string? IP { get; set; }
    }
}
