using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthors;

public class GetAuthorsHandler : IRequestHandler<GetAuthorsQuery, PagedResponse<AuthorListDto>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAuthorsHandler(
        IAuthorRepository authorRepository,
        ICurrentUserService currentUserService)
    {
        _authorRepository = authorRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<AuthorListDto>> Handle(GetAuthorsQuery request, CancellationToken cancellationToken)
    {
        var query = _authorRepository.GetQueryableSet()
            .AsNoTracking();

        // Search by name
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(a => a.FullName.ToLower().Contains(searchTerm));
        }

        // Only show approved authors for non-admin users
        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            query = query.Where(a => a.Status == ContentStatus.Approved);

        // Order by name
        query = query.OrderBy(a => a.FullName);

        var totalCount = await query.CountAsync(cancellationToken);

        var authors = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(a => new AuthorListDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Description = a.Description
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuthorListDto>
        {
            Items = authors,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
