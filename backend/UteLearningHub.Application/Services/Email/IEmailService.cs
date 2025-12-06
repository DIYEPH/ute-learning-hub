namespace UteLearningHub.Application.Services.Email;

public interface IEmailService
{
    /// <summary>
    /// Gửi email đơn giản
    /// </summary>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi email với nhiều người nhận
    /// </summary>
    Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi email password reset
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string resetUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi email welcome
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi email notification
    /// </summary>
    Task<bool> SendNotificationEmailAsync(string to, string title, string content, string? link = null, CancellationToken cancellationToken = default);
}

