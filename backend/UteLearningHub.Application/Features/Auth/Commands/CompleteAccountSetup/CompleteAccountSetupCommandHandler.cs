using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.CompleteAccountSetup;

public class CompleteAccountSetupCommandHandler 
    : IRequestHandler<CompleteAccountSetupCommand, CompleteAccountSetupResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public CompleteAccountSetupCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<CompleteAccountSetupResponse> Handle(
        CompleteAccountSetupCommand request, 
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException();

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();
        var errors = new List<string>();
        string? updatedUsername = null;
        bool passwordSet = false;

        // 1. Update username nếu có
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var (succeeded, usernameErrors) = await _identityService
                .UpdateUsernameAsync(userId, request.Username);
            
            if (succeeded)
            {
                updatedUsername = request.Username;
            }
            else
            {
                errors.AddRange(usernameErrors);
            }
        }

        // 2. Set password nếu có
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var (succeeded, passwordErrors) = await _identityService
                .SetPasswordAsync(userId, request.Password);
            
            if (succeeded)
            {
                passwordSet = true;
            }
            else
            {
                errors.AddRange(passwordErrors);
            }
        }

        // 3. Nếu cả 2 đều không có thì trả về lỗi
        if (string.IsNullOrWhiteSpace(request.Username) && 
            string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Either username or password must be provided");
        }

        return new CompleteAccountSetupResponse
        {
            Success = errors.Count == 0,
            Errors = errors,
            Username = updatedUsername,
            PasswordSet = passwordSet
        };
    }
}
