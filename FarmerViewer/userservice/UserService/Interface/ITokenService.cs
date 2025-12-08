namespace UserService.Interface
{
    
        public interface ITokenService
        {
            string GenerateToken(string fullName, int roleId, int bankId);
        }
}
