namespace WebApplication1.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderName { get; set; } = "Thư Viện Trường";
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
    }
}
