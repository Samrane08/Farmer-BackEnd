using Repository.Common;
using Repository.Entity;

namespace Repository.Interface;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);
    Task<Result> DeleteUserAsync(string userId);
    Task<Result> AddRoleByUserIdAsync(string userId, string roleName);
}
