namespace Repository.Entity
{
    public class EntityRoleMapping:BaseAuditableEntity
    {
        public int Id { get; set; }
        public string RoleId { get; set; }
        public int EntityTypeId { get; set; }
    }
}
