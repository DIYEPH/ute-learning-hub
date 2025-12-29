using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Major.Commands.CreateMajor;
using UteLearningHub.Application.Features.Major.Commands.UpdateMajor;
using UteLearningHub.Application.Features.Major.Queries.GetMajors;

namespace UteLearningHub.Application.Services.Major;

public interface IMajorService
{
    Task<MajorDetailDto> GetMajorByIdAsync(Guid id, bool isAdmin, CancellationToken ct);
    Task<PagedResponse<MajorDetailDto>> GetMajorsAsync(GetMajorsQuery request, bool isAdmin, CancellationToken ct);
    Task<MajorDetailDto> CreateAsync(Guid creatorId, CreateMajorCommand request, CancellationToken ct);
    Task<MajorDetailDto> UpdateAsync(Guid actorId, UpdateMajorCommand request, CancellationToken ct);
    Task SoftDeleteAsync(Guid majorId, Guid actorId, CancellationToken ct);
}