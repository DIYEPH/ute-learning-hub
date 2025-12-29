using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Author.Commands.CreateAuthor;
using UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;
using UteLearningHub.Application.Features.Author.Queries.GetAuthors;

namespace UteLearningHub.Application.Services.Author;

public interface IAuthorService
{
    Task<AuthorDetailDto> GetAuthorByIdAsync(Guid id, bool isAdmin, CancellationToken ct);
    Task<PagedResponse<AuthorDetailDto>> GetAuthorsAsync(GetAuthorsQuery request, bool isAdmin, CancellationToken ct);
    Task<AuthorDetailDto> CreateAsync(Guid creatorId, CreateAuthorCommand request, CancellationToken ct);
    Task<AuthorDetailDto> UpdateAsync(Guid actor, UpdateAuthorCommand request, CancellationToken ct);
    Task SoftDeleteAsync(Guid authorId, Guid actorId, CancellationToken ct);
}
