using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Major;

namespace UteLearningHub.Application.Features.Major.Queries.GetMajorById;

public class GetMajorByIdHandler(IMajorService majorService, ICurrentUserService currentUserService) : IRequestHandler<GetMajorByIdQuery, MajorDetailDto>
{
    private readonly IMajorService _majorService = majorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<MajorDetailDto> Handle(GetMajorByIdQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _majorService.GetMajorByIdAsync(request.Id, isAdmin, ct);
    }
}
