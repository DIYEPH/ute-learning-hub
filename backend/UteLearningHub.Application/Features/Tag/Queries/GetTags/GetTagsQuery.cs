using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTags;

public record GetTagsQuery : GetTagsRequest, IRequest<PagedResponse<TagDetailDto>>;
