using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Major;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Major.Commands.UpdateMajor;

public class UpdateMajorCommandHandler(IMajorService majorService, ICurrentUserService currentUserService) : IRequestHandler<UpdateMajorCommand, MajorDetailDto>
{
    private readonly IMajorService _majorService = majorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    public async Task<MajorDetailDto> Handle(UpdateMajorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can update majors");

        var actorId = _currentUserService.UserId!.Value;

        return await _majorService.UpdateAsync(actorId, request, cancellationToken);
    }
}
