using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Major.Commands.CreateMajor;

public record CreateMajorCommand : IRequest<MajorDetailDto>
{
    public Guid FacultyId { get; init; }
    public string MajorName { get; init; } = default!;
    public string MajorCode { get; init; } = default!;
}