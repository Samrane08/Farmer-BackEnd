namespace Model.Admin;

public class MenuModel
{
    public int? Id { get; set; }
    public string? MenuName { get; set; }
    public string? MenuNameMr { get; set; }
    public int? ParentId { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; }
    public List<MenuModel>? Submenu { get; set; }
}

public class EntityRoleMapModel
{
    public int Id { get; set; } 
    public string? EntityName { get; set; } 
    public string? RoleName { get; set; } 
    public int Status { get; set; } 
}

public class AdmistratorMenuModel
{
    public int MenuId { get; set; } = 0;
    public string MenuName { get; set; } = string.Empty;

    public bool IsAdded { get; set; } = false;
}

public class UpsertMenuMapping
{
    public List<int> MenuId { get; set; } 
    public int entitYmapingId { get; set; }

    
}