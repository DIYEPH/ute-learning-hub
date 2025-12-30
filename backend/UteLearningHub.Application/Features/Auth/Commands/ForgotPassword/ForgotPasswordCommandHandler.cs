using MediatR;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Services.Email;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;
    private readonly IPasswordResetLinkBuilder _passwordResetLinkBuilder;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IEmailService emailService,
        IIdentityService identityService,
        IPasswordResetLinkBuilder passwordResetLinkBuilder,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _emailService = emailService;
        _identityService = identityService;
        _passwordResetLinkBuilder = passwordResetLinkBuilder;
        _logger = logger;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return Unit.Value;
        }

        try
        {
            var resetToken = await _identityService.GeneratePasswordResetTokenAsync(user.Id);
            var resetUrl = _passwordResetLinkBuilder.BuildResetLink(request.Email, resetToken);

            _logger.LogInformation("Sending password reset email to {Email}", request.Email);
            
            var result = await _emailService.SendPasswordResetEmailAsync(request.Email, resetToken, resetUrl, cancellationToken);
            
            if (!result)
                _logger.LogError("Failed to send password reset email to {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", request.Email);
        }

        return Unit.Value;
    }
}
