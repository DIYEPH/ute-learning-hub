using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

    public MicrosoftTokenValidator(IOptions<MicrosoftAuthOptions> options)
    {
        _opts = options.Value;

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var authority = $"https://login.microsoftonline.com/{_opts.TenantId}/v2.0";
        var retriever = new HttpDocumentRetriever { RequireHttps = true };
        _cfgMgr = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            retriever);
    }

    public async Task<MicrosoftUserInfo?> ValidateTokenAsync(string idToken, CancellationToken ct = default)
    {
        OpenIdConnectConfiguration oidc;
        try
        {
            oidc = await _cfgMgr.GetConfigurationAsync(ct);
        }
        catch (Exception)
        {
            // log...
            return null;
        }

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = oidc.Issuer,               // << quan trọng
            ValidateAudience = true,
            ValidAudience = _opts.ClientId,          // ID token => clientId
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = oidc.SigningKeys
        };

        try
        {
            var principal = _handler.ValidateToken(idToken, parameters, out _);

            var email = principal.FindFirst("email")?.Value
                     ?? principal.FindFirst("preferred_username")?.Value
                     ?? principal.FindFirst(ClaimTypes.Email)?.Value;

            var name = principal.FindFirst("name")?.Value;
            var givenName = principal.FindFirst("given_name")?.Value
                         ?? principal.FindFirst(ClaimTypes.GivenName)?.Value;
            var familyName = principal.FindFirst("family_name")?.Value
                           ?? principal.FindFirst(ClaimTypes.Surname)?.Value;
            var oid = principal.FindFirst("oid")?.Value
                   ?? principal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(oid))
                return null;

            return new MicrosoftUserInfo(
                email ?? string.Empty,
                name ?? email ?? oid,
                givenName,
                familyName,
                oid);
        }
        catch (SecurityTokenException)
        {
            // token invalid/expired
            return null;
        }
        catch
        {
            // log...
            return null;
        }
    }
}
