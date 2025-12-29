using MediatR;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.ReviewDocumentFile;

public record ReviewDocumentFileCommand : IRequest
{
    public Guid DocumentFileId { get; init; }
    public ContentStatus Status { get; init; }
    public string? Reason { get; init; }
}
