namespace YarpGateWay.Models
{
    public class SessionVerifyModel
    {
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public DateTime? LoginAt { get; set; }
    }
}
