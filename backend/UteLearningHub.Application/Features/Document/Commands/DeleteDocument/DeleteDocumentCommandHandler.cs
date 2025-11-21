using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    public Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
