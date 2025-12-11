namespace UserService.Models.QueryDto
{
    public class LoginRequest
    {
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
        public string? DeviceFingerprint { get; set; }
    }
}
