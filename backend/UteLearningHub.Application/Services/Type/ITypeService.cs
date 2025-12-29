using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Type.Commands.CreateType;
using UteLearningHub.Application.Features.Type.Commands.UpdateType;
using UteLearningHub.Application.Features.Type.Queries.GetTypes;

namespace UteLearningHub.Application.Services.Type;

public interface ITypeService
{
    Task<TypeDetailDto> GetTypeByIdAsync(Guid id, bool isAdmin, CancellationToken ct);
    Task<PagedResponse<TypeDetailDto>> GetTypesAsync(GetTypesQuery request, bool isAdmin, CancellationToken ct);
    Task<TypeDetailDto> CreateAsync(Guid creatorId, CreateTypeCommand request, CancellationToken ct);
    Task<TypeDetailDto> UpdateAsync(Guid actorId, UpdateTypeCommand request, CancellationToken ct);
    Task SoftDeleteAsync(Guid typeId, Guid actorId, CancellationToken ct);
}