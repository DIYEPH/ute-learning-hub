using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocuments;

public record GetDocumentsQuery : GetDocumentsRequest, IRequest<PagedResponse<DocumentDto>>;
