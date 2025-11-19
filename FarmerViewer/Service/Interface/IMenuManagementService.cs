using Model.Admin;

namespace Service.Interface;

public interface IMenuManagementService
{
    Task<List<MenuModel>> GetMenuByRoleEntity(List<int> EntityMappingId);
    Task<MenuModel> UpsertAsync(MenuModel model);
    Task<List<MenuModel>> GetActiveMenuList();
    Task<List<MenuModel>> GetInActiveMenuList();
    Task<bool> MenuStatusUpdate(int MenuId,int Status);
    Task<bool> EntityRoleMenuMapping(int EntityRoleMappingId, int MenuId, int Status);

    Task<List<AdmistratorMenuModel>> GetMenuListFromRole(int entityRolemappingId);

    Task<bool>UpsertMenuMapping(UpsertMenuMapping model);
}