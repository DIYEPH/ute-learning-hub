using MediatR;

namespace UteLearningHub.Application.Features.Document.Queries.GetPendingFilesCount;

public record GetPendingFilesCountQuery : IRequest<int>;
