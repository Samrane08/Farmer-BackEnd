namespace UserService.Models.QueryDto
{
    public class LoginRequest
    {
        public string UserId { get; set; }   // user enters mobile/email/userid
        public string Password { get; set; }
    }
}
