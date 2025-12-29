using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Major;

namespace UteLearningHub.Application.Features.Major.Queries.GetMajors;

public class GetMajorsHandler(IMajorService majorService, ICurrentUserService currentUserService) : IRequestHandler<GetMajorsQuery, PagedResponse<MajorDetailDto>>
{
    private readonly IMajorService _majorService = majorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<PagedResponse<MajorDetailDto>> Handle(GetMajorsQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _majorService.GetMajorsAsync(request, isAdmin, ct);
    }
}
