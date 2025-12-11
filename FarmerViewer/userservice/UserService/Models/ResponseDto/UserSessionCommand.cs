namespace UserService.Models.ResponseDto
{
    public class UserSessionCommand
    {
        public long UserId { get; set; }
        public string Token { get; set; }
        public string Fingerprint { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public string IpAddress { get; set; }
        public string DeviceInfo { get; set; }
        public bool IsLoggedOut { get; set; }
        public string DeviceFingerprint { get; internal set; }
    }
}
