namespace UserService.Models.ResponseDto
{
    public class LoginResponse
    {
        public string FullName { get; set; }
        public int BankId { get; set; }
        public int RoleId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }
}
