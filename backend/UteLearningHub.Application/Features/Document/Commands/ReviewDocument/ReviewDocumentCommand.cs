using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.ReviewDocument;

public record ReviewDocumentCommand : ReviewDocumentRequest, IRequest<Unit>;
