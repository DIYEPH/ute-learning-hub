using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public record UpdateDocumentCommand : UpdateDocumentCommandRequest, IRequest<DocumentDetailDto>
{
    public Guid Id { get; init; }
}