using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Type;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypeById;

public class GetTypeByIdHandler(ITypeService typeService, ICurrentUserService currentUserService) : IRequestHandler<GetTypeByIdQuery, TypeDetailDto>
{
    private readonly ITypeService _typeService = typeService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TypeDetailDto> Handle(GetTypeByIdQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _typeService.GetTypeByIdAsync(request.Id, isAdmin, ct);
    }
}