using System.Web;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.Identity;

public class PasswordResetLinkBuilder : IPasswordResetLinkBuilder
{
    private readonly EmailOptions _emailOptions;

    public PasswordResetLinkBuilder(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public string BuildResetLink(string email, string token)
    {
        var baseUrl = _emailOptions.BaseUrl?.TrimEnd('/') ?? "http://localhost:3000";
        var encodedEmail = HttpUtility.UrlEncode(email);
        var encodedToken = HttpUtility.UrlEncode(token);

        return $"{baseUrl}/auth/reset-password?token={encodedToken}&email={encodedEmail}";
    }
}


