using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.Email;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(new[] { to }, subject, body, isHtml, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        try
        {
            // Nếu không có cấu hình email, chỉ log và return true (development mode)
            if (string.IsNullOrWhiteSpace(_options.SmtpServer) || string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                _logger.LogWarning("Email service not configured. Skipping email send to {Recipients}. Subject: {Subject}",
                    string.Join(", ", to), subject);
                return true;
            }

            using var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            foreach (var recipient in to)
            {
                if (!string.IsNullOrWhiteSpace(recipient))
                {
                    message.To.Add(recipient);
                }
            }

            if (message.To.Count == 0)
            {
                _logger.LogWarning("No valid recipients for email. Subject: {Subject}", subject);
                return false;
            }

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully to {Recipients}. Subject: {Subject}",
                string.Join(", ", to), subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}. Subject: {Subject}",
                string.Join(", ", to), subject);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string resetUrl, CancellationToken cancellationToken = default)
    {
        var subject = "Đặt lại mật khẩu - UTE Learning Hub";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2563eb; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ padding: 30px; background-color: #f9fafb; }}
        .button {{ display: inline-block; padding: 14px 28px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 8px; font-weight: bold; }}
        .footer {{ padding: 20px; text-align: center; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>UTE Learning Hub</h1>
        </div>
        <div class='content'>
            <h2>Đặt lại mật khẩu</h2>
            <p>Xin chào,</p>
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình. Click vào nút bên dưới để tiếp tục:</p>
            <p style='text-align: center; margin: 30px 0;'>
                <a href='{resetUrl}' class='button'>Đặt lại mật khẩu</a>
            </p>
            <p><strong>Lưu ý:</strong> Link này sẽ hết hạn sau 15 phút.</p>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
        </div>
        <div class='footer'>
            <p>© 2025 UTE Learning Hub. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, isHtml: true, cancellationToken);
    }

    public async Task<bool> SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default)
    {
        var subject = "Chào mừng đến với UTE Learning Hub";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2563eb; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .footer {{ padding: 20px; text-align: center; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>UTE Learning Hub</h1>
        </div>
        <div class='content'>
            <h2>Chào mừng, {userName}!</h2>
            <p>Cảm ơn bạn đã tham gia UTE Learning Hub - nền tảng học tập và chia sẻ tài liệu của sinh viên Đại học Sư phạm Kỹ thuật Đà Nẵng.</p>
            <p>Bạn có thể:</p>
            <ul>
                <li>1. Tìm kiếm và tải xuống tài liệu học tập</li>
                <li>2. Đóng góp tài liệu của riêng bạn</li>
                <li>3. Tham gia các cuộc trò chuyện và thảo luận</li>
                <li>4. Đánh giá và nhận xét về tài liệu</li>
            </ul>
            <p>Chúc bạn có trải nghiệm học tập tuyệt vời!</p>
        </div>
        <div class='footer'>
            <p>© 2025 UTE Learning Hub. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, isHtml: true, cancellationToken);
    }

    public async Task<bool> SendNotificationEmailAsync(string to, string title, string content, string? link = null, CancellationToken cancellationToken = default)
    {
        var subject = $"Thông báo: {title}";
        var linkHtml = !string.IsNullOrWhiteSpace(link)
            ? $@"<p style='text-align: center;'>
                <a href='{link}' class='button' style='display: inline-block; padding: 12px 24px; background-color: #2563eb; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0;'>Xem chi tiết</a>
               </p>"
            : string.Empty;

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2563eb; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .footer {{ padding: 20px; text-align: center; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>UTE Learning Hub</h1>
        </div>
        <div class='content'>
            <h2>{title}</h2>
            <div>{content}</div>
            {linkHtml}
        </div>
        <div class='footer'>
            <p>© 2025 UTE Learning Hub. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(to, subject, body, isHtml: true, cancellationToken);
    }
}

