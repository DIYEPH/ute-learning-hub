using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocument;

public record DeleteDocumentCommand : DeleteDocumentRequest, IRequest<Unit>;
