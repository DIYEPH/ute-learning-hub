using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Profile;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDetailDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IProfileService _profileService;

    public UpdateProfileCommandHandler(
        ICurrentUserService currentUserService,
        IProfileService profileService)
    {
        _currentUserService = currentUserService;
        _profileService = profileService;
    }

    public async Task<ProfileDetailDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.UserId!.Value;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var isOwner = actorId == request.Id;

        if (!isAdmin && !isOwner)
            throw new ForbiddenException("Only admin or owner can update profile");

        return await _profileService.UpdateAsync(actorId, request, cancellationToken);
    }
}