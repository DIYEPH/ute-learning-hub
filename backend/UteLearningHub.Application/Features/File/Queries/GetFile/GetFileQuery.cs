using MediatR;
using UteLearningHub.Application.Common.Results;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.File.Queries.GetFile;

public record GetFileByIdQuery : IRequest<FileStreamResult>
{
    public Guid FileId { get; init; }
    public FileRequestType FileRequestType { get; set; }
}

