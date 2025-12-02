using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public record AddDocumentFileCommand : AddDocumentFileRequest, IRequest<DocumentDetailDto>;
