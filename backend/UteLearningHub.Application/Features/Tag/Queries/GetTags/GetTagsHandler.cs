using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Tag;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTags;

public class GetTagsHandler(ITagService tagService, ICurrentUserService currentUserService) : IRequestHandler<GetTagsQuery, PagedResponse<TagDetailDto>>
{
    private readonly ITagService _tagService = tagService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<PagedResponse<TagDetailDto>> Handle(GetTagsQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _tagService.GetTagsAsync(request, isAdmin, ct);
    }
}