using UserService.Models.ResponseDto;
using UserService.Models.QueryDto;

namespace UserService.Interface
{
    public interface IAuthRepository
    {
        Task<LoginResponse> CheckLoginAsync(LoginRequest loginRequest);
    }
}
