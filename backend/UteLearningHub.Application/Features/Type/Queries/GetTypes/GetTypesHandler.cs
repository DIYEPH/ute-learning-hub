using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Type;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypes;

public class GetTypesHandler(ITypeService typeService, ICurrentUserService currentUserService) : IRequestHandler<GetTypesQuery, PagedResponse<TypeDetailDto>>
{
    private readonly ITypeService _typeService = typeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<PagedResponse<TypeDetailDto>> Handle(GetTypesQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _typeService.GetTypesAsync(request, isAdmin, ct);
    }
}