using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetReadingHistory;

public record GetReadingHistoryQuery : PagedRequest, IRequest<PagedResponse<ReadingHistoryItemDto>>;
