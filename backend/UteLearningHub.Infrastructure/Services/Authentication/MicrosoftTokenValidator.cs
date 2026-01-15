using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.Authentication;

public class MicrosoftTokenValidator : IMicrosoftTokenValidator
{
    private readonly MicrosoftAuthOptions _opts;
    private readonly JwtSecurityTokenHandler _handler = new();
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _cfgMgr;
    private readonly ILogger<MicrosoftTokenValidator> _logger;

    // Retry configuration
    private const int MaxRetries = 3;
    private static readonly TimeSpan[] RetryDelays = { 
        TimeSpan.FromMilliseconds(100), 
        TimeSpan.FromMilliseconds(500), 
        TimeSpan.FromSeconds(1) 
    };

    public MicrosoftTokenValidator(
        IOptions<MicrosoftAuthOptions> options,
        ILogger<MicrosoftTokenValidator> logger)
    {
        _opts = options.Value;
        _logger = logger;

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var authority = $"https://login.microsoftonline.com/{_opts.TenantId}/v2.0";
        var retriever = new HttpDocumentRetriever { RequireHttps = true };
        
        _cfgMgr = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            retriever)
        {
            // Auto refresh interval: refresh keys mỗi 12 giờ
            AutomaticRefreshInterval = TimeSpan.FromHours(12),
            // Minimum time between refresh attempts after a failure
            RefreshInterval = TimeSpan.FromMinutes(5)
        };
    }

    public async Task<MicrosoftUserInfo?> ValidateTokenAsync(string idToken, CancellationToken ct = default)
    {
        OpenIdConnectConfiguration? oidc = null;
        Exception? lastException = null;

        // Retry logic với exponential backoff
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    // Force refresh configuration trước khi retry
                    _cfgMgr.RequestRefresh();
                    _logger.LogWarning(
                        "Microsoft OIDC config fetch retry {Attempt}/{MaxRetries} after RequestRefresh", 
                        attempt, MaxRetries);
                    
                    // Delay trước khi retry
                    await Task.Delay(RetryDelays[Math.Min(attempt - 1, RetryDelays.Length - 1)], ct);
                }

                oidc = await _cfgMgr.GetConfigurationAsync(ct);
                break; // Thành công, thoát vòng lặp
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, 
                    "Failed to get Microsoft OIDC configuration on attempt {Attempt}", attempt + 1);
            }
        }

        if (oidc == null)
        {
            _logger.LogError(lastException, 
                "Failed to get Microsoft OIDC configuration after {MaxRetries} retries", MaxRetries + 1);
            return null;
        }

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = oidc.Issuer,
            ValidateAudience = true,
            ValidAudience = _opts.ClientId,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = oidc.SigningKeys
        };

        try
        {
            var principal = _handler.ValidateToken(idToken, parameters, out _);

            var email = principal.FindFirst("preferred_username")?.Value
                     ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            var name = principal.FindFirst("name")?.Value;

            var oid = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(oid) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
            {
                _logger.LogWarning(
                    "Microsoft token validation missing claims: oid={HasOid}, email={HasEmail}, name={HasName}",
                    !string.IsNullOrEmpty(oid), !string.IsNullOrEmpty(email), !string.IsNullOrEmpty(name));
                return null;
            }

            return new MicrosoftUserInfo(email, name, oid);
        }
        catch (SecurityTokenSignatureKeyNotFoundException ex)
        {
            // Signing key không tìm thấy - cần refresh configuration
            _logger.LogWarning(ex, "Signing key not found, requesting configuration refresh");
            _cfgMgr.RequestRefresh();
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Microsoft token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Microsoft token validation");
            return null;
        }
    }
}
