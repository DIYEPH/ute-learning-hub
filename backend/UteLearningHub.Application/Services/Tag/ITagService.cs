using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Tag.Commands.UpdateTag;
using UteLearningHub.Application.Features.Tag.Queries.GetTags;

namespace UteLearningHub.Application.Services.Tag;

public interface ITagService
{
    Task<TagDetailDto> GetTagByIdAsync(Guid id, bool isAdmin, CancellationToken ct);
    Task<PagedResponse<TagDetailDto>> GetTagsAsync(GetTagsQuery request, bool isAdmin, CancellationToken ct);
    Task<TagDetailDto> CreateAsync(Guid creatorId, string tagName, CancellationToken ct);
    Task<TagDetailDto> UpdateAsync(Guid actorId, UpdateTagCommand request, CancellationToken ct);
    Task SoftDeleteAsync(Guid tagId, Guid actorId, CancellationToken ct);
}
