namespace UserService.Models.QueryDto
{
    public class UserSessionQuery
    {
        public long SessionId { get; set; }
        public long UserId { get; set; }
        public string TokenHash { get; set; }
        public string Fingerprint { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public string DeviceFingerprint { get; set; }
        public bool IsActive { get; set; }
    }

}
