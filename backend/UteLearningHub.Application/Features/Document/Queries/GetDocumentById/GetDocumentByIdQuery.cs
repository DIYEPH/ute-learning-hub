using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentById;

public record GetDocumentByIdQuery : IRequest<DocumentDetailDto>
{
    public Guid Id { get; init; }
}