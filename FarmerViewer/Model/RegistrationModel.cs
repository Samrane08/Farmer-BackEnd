using System.ComponentModel.DataAnnotations;

namespace Model;

public class RegistrationModel
{
    public string UdiseNo { get; set; }
    //public string Password { get; set; }
    public string SchoolName { get; set; }
    public string OrgName { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public bool IsNew { get; set; }
}

public class NewUDISEEntryModel
{
    public long UdiseNo { get; set; }
    public string SchoolName { get; set; }=string.Empty;
    public string OrgName { get; set; } = string.Empty;
    public bool IsNew {  get; set; }
}

public class NewStudentRegistrationModel
{
    [Required]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Username must be between 5 and 50 characters.")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
    public string Mobile { get; set; } = string.Empty;

}

public class CheckUserExistenceModel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
