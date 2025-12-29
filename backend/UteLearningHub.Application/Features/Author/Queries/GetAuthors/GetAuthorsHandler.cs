using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthors;

public class GetAuthorsHandler(IAuthorService authorService, ICurrentUserService currentUserService) : IRequestHandler<GetAuthorsQuery, PagedResponse<AuthorDetailDto>>
{
    private readonly IAuthorService _authorService = authorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<PagedResponse<AuthorDetailDto>> Handle(GetAuthorsQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _authorService.GetAuthorsAsync(request, isAdmin, ct);
    }
}
