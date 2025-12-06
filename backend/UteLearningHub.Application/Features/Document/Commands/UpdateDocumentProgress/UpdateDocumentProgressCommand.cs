using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;

public record UpdateDocumentProgressCommand : UpdateDocumentProgressRequest, IRequest<Unit>;

