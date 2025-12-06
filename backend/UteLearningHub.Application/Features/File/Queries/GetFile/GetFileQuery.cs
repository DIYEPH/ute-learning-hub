using MediatR;

namespace UteLearningHub.Application.Features.File.Queries.GetFile;

public record GetFileQuery : IRequest<GetFileResponse>
{
    public Guid FileId { get; init; }
}

