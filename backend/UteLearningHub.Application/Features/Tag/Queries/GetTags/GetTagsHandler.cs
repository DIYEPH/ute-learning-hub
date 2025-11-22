using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTags;

public class GetTagsHandler : IRequestHandler<GetTagsQuery, PagedResponse<TagDto>>
{
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTagsHandler(ITagRepository tagRepository, ICurrentUserService currentUserService)
    {
        _tagRepository = tagRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _tagRepository.GetQueryableSet()
            .AsNoTracking();

        // Search by name
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t => t.TagName.ToLower().Contains(searchTerm));
        }

        // Only show approved tags for public users, admin can see all
        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!isAdmin)
        {
            query = query.Where(t => t.ReviewStatus == ReviewStatus.Approved);
        }

        // Order by name
        query = query.OrderBy(t => t.TagName);

        var totalCount = await query.CountAsync(cancellationToken);

        var tags = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(t => new TagDto
            {
                Id = t.Id,
                TagName = t.TagName
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<TagDto>
        {
            Items = tags,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}