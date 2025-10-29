namespace Progra3_TPFinal_19B.Models.Email
{
    public class EmailSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string From { get; set; } = "";
        public string FromName { get; set; } = "";
        public bool UseStartTls { get; set; } = true;
    }
}
