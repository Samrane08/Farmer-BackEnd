namespace UserService.Model.Request
{
    public class EmailSender
    {
        public string? key { get; set; }
        public string? to { get; set; }
        public List<string>? param { get; set; }
    }
}
