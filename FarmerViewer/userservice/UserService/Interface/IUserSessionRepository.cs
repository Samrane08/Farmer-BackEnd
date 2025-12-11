using UserService.Models.QueryDto;
using UserService.Models.ResponseDto;

namespace UserService.Interface
{
    public interface IUserSessionRepository
    {
        Task<int> CreateOrUpdateSessionAsync(UserSessionCommand cmd);
        Task<UserSessionQuery?> GetActiveSessionByUserAsync(long userId);
        Task<UserSessionQuery?> GetSessionByTokenAsync(string token);
        Task<int> UpdateLastSeenByTokenAsync(string token);
        Task<int> LogoutByTokenAsync(string token);
        Task<int> InvalidateSessionsOlderThanAsync(DateTime cutoff);
    }

}
