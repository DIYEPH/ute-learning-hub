using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public record UpdateDocumentCommand : UpdateDocumentRequest, IRequest<DocumentDetailDto>;