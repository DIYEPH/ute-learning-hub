using MediatR;

namespace UteLearningHub.Application.Features.DocumentFiles.Commands.ReviewDocumentFile;

public record ReviewDocumentFileCommand : ReviewDocumentFileRequest, IRequest<Unit>;
