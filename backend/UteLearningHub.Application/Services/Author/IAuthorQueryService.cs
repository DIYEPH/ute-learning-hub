using UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

namespace UteLearningHub.Application.Services.Author;

public interface IAuthorQueryService
{
    Task<AuthorDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
