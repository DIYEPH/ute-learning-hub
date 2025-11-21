using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;

public class DeleteDocumentsCommandHandler : IRequestHandler<DeleteDocumentsCommand, Unit>
{
    public Task<Unit> Handle(DeleteDocumentsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
