using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Profile;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDetailDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IProfileService _profileService;
    private readonly IVectorMaintenanceService _vectorService;

    public UpdateProfileCommandHandler(
        ICurrentUserService currentUserService,
        IProfileService profileService,
        IVectorMaintenanceService vectorService)
    {
        _currentUserService = currentUserService;
        _profileService = profileService;
        _vectorService = vectorService;
    }

    public async Task<ProfileDetailDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.UserId!.Value;
        var isAdmin = _currentUserService.IsInRole("Admin");
        
        var targetId = request.Id == Guid.Empty ? actorId : request.Id;
        var isOwner = actorId == targetId;

        if (!isAdmin && !isOwner)
            throw new ForbiddenException("Only admin or owner can update profile");

        var commandWithId = request with { Id = targetId };
        var result = await _profileService.UpdateAsync(actorId, commandWithId, cancellationToken);

        try { await _vectorService.UpdateUserVectorAsync(targetId, cancellationToken); }
        catch { }

        return result;
    }
}
