using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Tag;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTagById;

public class GetTagByIdHandler(ITagService tagService, ICurrentUserService currentUserService) : IRequestHandler<GetTagByIdQuery, TagDetailDto>
{
    private readonly ITagService _tagService = tagService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TagDetailDto> Handle(GetTagByIdQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _tagService.GetTagByIdAsync(request.Id, isAdmin, ct);
    }
}
