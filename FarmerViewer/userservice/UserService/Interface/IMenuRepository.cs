using UserService.Models.ResponseDto;

namespace UserService.Interface
{
    public interface IMenuRepository
    {
        Task<List<MenuResponseDto>> GetMenuByRoleIdAsync(int roleId);
        Task<List<SubMenuResponseDto>> GetSubMenusAsync(int roleId, int menuId);
    }
}