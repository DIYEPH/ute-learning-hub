using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, errors) = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

        if (!succeeded)
        {
            var errorMessage = string.Join(", ", errors);
            throw new BadRequestException($"Failed to reset password: {errorMessage}");
        }

        return Unit.Value;
    }
}

