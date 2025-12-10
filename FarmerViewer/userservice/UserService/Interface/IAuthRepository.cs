using UserService.Models.ResponseDto;
using UserService.Models.QueryDto;

namespace UserService.Interface
{
    public interface IAuthRepository
    {
        Task<LoginResponse> CheckLoginAsync(string userId, string Password);
    }
}
