using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Major;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Major.Commands.CreateMajor;

public class CreateMajorCommandHandler(IMajorService majorService, ICurrentUserService currentUserService) : IRequestHandler<CreateMajorCommand, MajorDetailDto>
{
    private readonly IMajorService _majorService = majorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<MajorDetailDto> Handle(CreateMajorCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can create majors");

        var actorId = _currentUserService.UserId!.Value;

        return await _majorService.CreateAsync(actorId, request, ct);
    }
}
