using UserService.Models.ResponseDto;

namespace UserService.Interface
{
    public interface IMenuRepository
    {
        Task<List<MenuResponseDto>> GetMenuByRoleIdAsync(int roleId);
    }
}