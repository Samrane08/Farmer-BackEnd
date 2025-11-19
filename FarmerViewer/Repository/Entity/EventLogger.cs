using System.ComponentModel.DataAnnotations;

namespace Repository.Entity
{
    public class EventLogger
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public string? IPAddress { get; set; }
        public string? RequestURL { get; set; }
        public string? HttpMethod { get; set; }
        public string? AbsoluteURL { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
