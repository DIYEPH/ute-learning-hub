using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

public class GetAuthorByIdHandler(IAuthorService authorService, ICurrentUserService currentUserService) : IRequestHandler<GetAuthorByIdQuery, AuthorDetailDto>
{
    private readonly IAuthorService _authorService = authorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<AuthorDetailDto> Handle(GetAuthorByIdQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _authorService.GetAuthorByIdAsync(request.Id, isAdmin, ct);
    }
}
