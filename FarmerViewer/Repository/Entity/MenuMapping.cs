namespace Repository.Entity
{
    public class MenuMapping : BaseAuditableEntity
    {
        public int Id { get; set; }
        public int EntityMappingId { get; set; }
        public int MenuId { get; set; }
    }
}
