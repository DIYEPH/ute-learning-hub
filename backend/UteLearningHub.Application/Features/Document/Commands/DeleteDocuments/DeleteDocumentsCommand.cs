using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;

public record DeleteDocumentsCommand : DeleteDocumentsRequest, IRequest<Unit>;
