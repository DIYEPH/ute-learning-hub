using MediatR;
using UteLearningHub.Application.Services.Email;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;
    private readonly IPasswordResetLinkBuilder _passwordResetLinkBuilder;

    public ForgotPasswordCommandHandler(
        IEmailService emailService,
        IIdentityService identityService,
        IPasswordResetLinkBuilder passwordResetLinkBuilder)
    {
        _emailService = emailService;
        _identityService = identityService;
        _passwordResetLinkBuilder = passwordResetLinkBuilder;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            return Unit.Value;
        }

        var resetToken = await _identityService.GeneratePasswordResetTokenAsync(user.Id);
        var resetUrl = _passwordResetLinkBuilder.BuildResetLink(request.Email, resetToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendPasswordResetEmailAsync(request.Email, resetToken, resetUrl, cancellationToken);
            }
            catch
            {
                // Log error nhưng không throw để không ảnh hưởng đến response
            }
        }, cancellationToken);

        return Unit.Value;
    }
}

