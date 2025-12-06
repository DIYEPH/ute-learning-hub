using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentProgress;

public record GetDocumentProgressQuery : IRequest<DocumentProgressDto>
{
    public Guid DocumentFileId { get; init; }
}

