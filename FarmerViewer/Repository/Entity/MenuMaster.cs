namespace Repository.Entity
{
    public class MenuMaster :BaseAuditableEntity
    {
        public int Id { get; set; }
        public string? MenuName { get; set; }
        public string? MenuNameMr { get; set; }
        public int? ParentId { get; set; }
        public string? Url { get; set; } 
        public string? Icon { get; set; }
        public int Sort { get; set; }
    }
}
