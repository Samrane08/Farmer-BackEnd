namespace Repository.Entity;

public class EntityType:BaseAuditableEntity
{
    public int Id { get; set; }
    public string? EntityTypeName { get; set; }
}
