using Helper;
using Microsoft.EntityFrameworkCore;
using Model.Admin;
using Repository.Data;
using Repository.Entity;
using Repository.Enums;
using Service.Interface;

namespace Service.Implementation;

public class MenuManagementService : IMenuManagementService
{
    private readonly ApplicationDbContext context;

    public MenuManagementService(ApplicationDbContext context)
    {
        this.context = context;
    }

    private List<MenuModel> GetMenuTree(List<MenuModel> list, int? parent)
    {
        return list.Where(x => x.ParentId == parent).Select(x => new MenuModel
        {
            Id = x.Id,
            ParentId = x.ParentId,
            MenuName = x.MenuName,
            MenuNameMr = x.MenuNameMr,
            Url = x.Url,
            Icon = x.Icon,
            Sort = x.Sort,
            Submenu = GetMenuTree(list, x.Id)
        }).OrderBy(x => x.Sort).ToList();
    }

    public async Task<List<MenuModel>> GetMenuByRoleEntity(List<int> EntityMappingId)
    {
        try
        {
            var menuIds = await context.MenuMapping.Where(x => EntityMappingId.Contains(x.EntityMappingId))
                                                   .Select(x => x.MenuId).ToListAsync();
            var menus = await context.MenuMaster.Where(x => menuIds.Contains(x.Id) && x.Status == Repository.Enums.Status.Active)
                                    .Select(x => new MenuModel
                                    {
                                        Id = x.Id,
                                        ParentId = x.ParentId,
                                        MenuName = x.MenuName,
                                        MenuNameMr = x.MenuNameMr,
                                        Url = x.Url,
                                        Icon = x.Icon,
                                        Sort = x.Sort
                                    }).OrderBy(x => x.Sort).ToListAsync();
            var menustree = GetMenuTree(menus, null);
            return menustree.OrderBy(x => x.Sort).ToList();
        }
        
        catch(Exception ex)
        {
            throw ex;
        }
    }

    public async Task<MenuModel> UpsertAsync(MenuModel model)
    {
        var result = await context.MenuMaster.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
        if (result != null)
        {
            result.Sort = model.Sort;
            result.MenuName = model.MenuName;
            result.MenuNameMr = model.MenuNameMr;
            result.Url = model.Url;
            result.Icon = model.Icon;
            result.ParentId = model.ParentId;
        }
        else
        {
            var entity = model.ToType<MenuMaster>();
            entity.Status = Repository.Enums.Status.Active;
            await context.MenuMaster.AddAsync(entity);
            model.Id = entity.Id;
        }
        await context.SaveChangesAsync();
        return model;
    }

    public async Task<List<MenuModel>> GetActiveMenuList()
    {
        var menus = await context.MenuMaster.Where(x => x.Status == Repository.Enums.Status.Active)
                               .Select(x => new MenuModel
                               {
                                   Id = x.Id,
                                   ParentId = x.ParentId,
                                   MenuName = x.MenuName,
                                   MenuNameMr = x.MenuNameMr,
                                   Url = x.Url,
                                   Icon = x.Icon,
                                   Sort = x.Sort
                               }).ToListAsync();

        var menustree = GetMenuTree(menus, null);
        return menustree.OrderBy(x => x.Sort).ToList();
    }

    public async Task<List<MenuModel>> GetInActiveMenuList()
    {
        var menus = await context.MenuMaster.Where(x => x.Status == Repository.Enums.Status.Inactive)
                               .Select(x => new MenuModel
                               {
                                   Id = x.Id,
                                   ParentId = x.ParentId,
                                   MenuName = x.MenuName,
                                   MenuNameMr = x.MenuNameMr,
                                   Url = x.Url,
                                   Icon = x.Icon,
                                   Sort = x.Sort
                               }).ToListAsync();

        var menustree = GetMenuTree(menus, null);
        return menustree.OrderBy(x => x.Sort).ToList();
    }

    public async Task<bool> MenuStatusUpdate(int MenuId, int Status)
    {
        try
        {
            var result = await context.MenuMaster.Where(x => x.Id == MenuId).FirstOrDefaultAsync();
            if (result != null)
            {
                result.Status = (Repository.Enums.Status)Status;
            }
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return true;
        }
    }

    public async Task<bool> EntityRoleMenuMapping(int EntityRoleMappingId, int MenuId, int Status)
    {
        try
        {
            var result = context.MenuMapping.Where(x => x.EntityMappingId == EntityRoleMappingId && x.MenuId == MenuId).FirstOrDefault();
            if (result != null)
            {
                result.Status = (Repository.Enums.Status)Status;
            }
            else
            {
                var entity = new MenuMapping
                {
                    EntityMappingId = EntityRoleMappingId,
                    MenuId = MenuId,
                    Status = (Repository.Enums.Status)Status
                };
                await context.MenuMapping.AddAsync(entity);
            }
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {

            return false;
        }
    }

    public async Task<List<AdmistratorMenuModel>> GetMenuListFromRole(int entitymappingId)
    {
        try
        {
            var result = await (
            from a in context.MenuMaster
            join b in context.MenuMapping
                on a.Id equals b.MenuId into menuMappingGroup
            from b in menuMappingGroup.DefaultIfEmpty()
            
            select new AdmistratorMenuModel
            {
                MenuId = a.Id,
                MenuName = a.MenuName,
                IsAdded = b != null && b.EntityMappingId == entitymappingId
            }).ToListAsync();

            return result;
        }catch(Exception ex)
        {
           return new List<AdmistratorMenuModel>();
        }
    }

    public async Task<bool> UpsertMenuMapping(UpsertMenuMapping model)
    {
        try
        {
            List<MenuMapping> _menumappingList = new List<MenuMapping>();
            var NonexistingValues = await context.MenuMapping
             .Where(x => x.EntityMappingId == model.entitYmapingId && !model.MenuId.Contains(x.Id))
            .Select(x => x.MenuId)
            .ToListAsync();

            foreach (var o in NonexistingValues)
            {
                var m = new MenuMapping();
                m.EntityMappingId = model.entitYmapingId;
                m.MenuId = o;
                m.Status = Status.Active;
                m.Created = DateTime.UtcNow;
                _menumappingList.Add(m);

            }
            if (_menumappingList != null && _menumappingList.Count > 0)
            {
                await context.MenuMapping.AddRangeAsync(_menumappingList);
            }
            return true;
        }catch(Exception ex)
        {
            throw;
        }
    }
}
