namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "UTE Learning Hub";
    public string BaseUrl { get; set; } = "http://localhost:3000"; // Frontend URL để tạo reset link
}

