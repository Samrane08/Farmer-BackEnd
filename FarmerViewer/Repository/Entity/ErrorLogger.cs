using System.ComponentModel.DataAnnotations;

namespace Repository.Entity
{
    public class ErrorLogger
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? ErrorAt { get; set; }
        public string? Exception { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
