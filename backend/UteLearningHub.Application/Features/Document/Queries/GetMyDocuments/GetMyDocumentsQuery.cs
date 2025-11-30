using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetMyDocuments;

public record GetMyDocumentsQuery : GetMyDocumentsRequest, IRequest<PagedResponse<DocumentDto>>;

