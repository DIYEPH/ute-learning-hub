using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentById;

public record GetDocumentByIdQuery : GetDocumentByIdRequest, IRequest<DocumentDetailDto>;