using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthors;

public class GetAuthorsHandler : IRequestHandler<GetAuthorsQuery, PagedResponse<AuthorListDto>>
{
    private readonly IAuthorRepository _authorRepository;

    public GetAuthorsHandler(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<PagedResponse<AuthorListDto>> Handle(GetAuthorsQuery request, CancellationToken cancellationToken)
    {
        var query = _authorRepository.GetQueryableSet()
            .AsNoTracking();

        // Filter by IsDeleted status (default: only active items)
        if (request.IsDeleted.HasValue)
            query = query.Where(a => a.IsDeleted == request.IsDeleted.Value);
        else
            query = query.Where(a => !a.IsDeleted);

        // Search by name
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(a => a.FullName.ToLower().Contains(searchTerm));
        }

        // Only show approved authors
        query = query.Where(a => a.ReviewStatus == ReviewStatus.Approved);

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
