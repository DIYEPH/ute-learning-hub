using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

public class GetAuthorByIdHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDetailDto>
{
    private readonly IAuthorRepository _authorRepository;

    public GetAuthorByIdHandler(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<AuthorDetailDto> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetByIdAsync(request.Id, disableTracking: true, cancellationToken);

        if (author == null || author.IsDeleted)
            throw new NotFoundException($"Author with id {request.Id} not found");

        if (author.ReviewStatus != ReviewStatus.Approved)
            throw new NotFoundException($"Author with id {request.Id} not found");

        var documentCount = await _authorRepository.GetQueryableSet()
            .Where(a => a.Id == request.Id)
            .Select(a => a.DocumentAuthors.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new AuthorDetailDto
        {
            Id = author.Id,
            FullName = author.FullName,
            Description = author.Description,
            DocumentCount = documentCount
        };
    }
}
