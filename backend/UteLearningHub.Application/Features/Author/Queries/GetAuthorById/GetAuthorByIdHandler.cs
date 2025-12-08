using MediatR;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

public class GetAuthorByIdHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDetailDto>
{
    private readonly IAuthorQueryService _authorQueryService;

    public GetAuthorByIdHandler(IAuthorQueryService authorQueryService)
    {
        _authorQueryService = authorQueryService;
    }

    public async Task<AuthorDetailDto> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var author = await _authorQueryService.GetByIdAsync(request.Id, cancellationToken);
        return author ?? throw new NotFoundException($"Author with id {request.Id} not found");
    }
}
