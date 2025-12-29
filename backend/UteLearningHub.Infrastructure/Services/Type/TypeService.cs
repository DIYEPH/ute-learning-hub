using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Type.Commands.CreateType;
using UteLearningHub.Application.Features.Type.Commands.UpdateType;
using UteLearningHub.Application.Features.Type.Queries.GetTypes;
using UteLearningHub.Application.Services.Type;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

using TypeEntity = UteLearningHub.Domain.Entities.Type;

namespace UteLearningHub.Infrastructure.Services.Type;

public class TypeService(ITypeRepository typeRepository, IDateTimeProvider dateTimeProvider) : ITypeService
{
    private readonly ITypeRepository _typeRepository = typeRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    public async Task<TypeDetailDto> CreateAsync(Guid creatorId, CreateTypeCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TypeName))
            throw new BadRequestException("Name cannot be empty");

        var exists = await _typeRepository
            .GetQueryableSet()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => EF.Functions.Like(f.TypeName, request.TypeName.Trim()), ct);

        if (exists != null)
            throw new BadRequestException($"Type with name '{request.TypeName}' already exists");

        var type = new TypeEntity
        {
            Id = Guid.NewGuid(),
            TypeName = request.TypeName,
            CreatedAt = _dateTimeProvider.OffsetUtcNow,
            CreatedById = creatorId,
        };

        _typeRepository.Add(type);
        await _typeRepository.UnitOfWork.SaveChangesAsync(ct);

        return new TypeDetailDto
        {
            Id = type.Id,
            TypeName = type.TypeName,
            CreatedById = type.CreatedById,
            CreatedAt = _dateTimeProvider.OffsetNow
        };
    }

    public async Task<TypeDetailDto> GetTypeByIdAsync(Guid id, bool isAdmin, CancellationToken ct)
    {
        var query = _typeRepository.GetQueryableSet()
            .Where(m => m.Id == id);

        if (isAdmin)
            query = query.IgnoreQueryFilters();

        var dto = await query
            .Select(t => new TypeDetailDto
            {
                Id = t.Id,
                TypeName = t.TypeName,
                CreatedById = t.CreatedById,
                DocumentCount = t.Documents.Count(d => isAdmin || !d.IsDeleted),
                IsDeleted = isAdmin ? t.IsDeleted : null,
                UpdatedAt = isAdmin ? t.UpdatedAt : null,
                CreatedAt = isAdmin ? t.CreatedAt : null,
                DeletedById = isAdmin ? t.DeletedById : null,
                DeletedAt = isAdmin ? t.DeletedAt : null,
            })
            .FirstOrDefaultAsync(ct);

        if (dto == null)
            throw new NotFoundException($"Type with id {id} not found");

        return dto;
    }

    public async Task<PagedResponse<TypeDetailDto>> GetTypesAsync(GetTypesQuery request, bool isAdmin, CancellationToken ct)
    {
        var query = _typeRepository.GetQueryableSet();

        if (isAdmin)
        {
            query = query.IgnoreQueryFilters();
            if (request.IsDeleted.HasValue)
                query = query.Where(m => m.IsDeleted == request.IsDeleted.Value);
        }

        query = query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(f => EF.Functions.Like(f.TypeName, term));
        }

        query = query.OrderBy(a => a.TypeName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(t => new TypeDetailDto
            {
                Id = t.Id,
                TypeName = t.TypeName,
                CreatedById = t.CreatedById,
                DocumentCount = t.Documents.Count(d => isAdmin || !d.IsDeleted),
                IsDeleted = isAdmin ? t.IsDeleted : null,
                UpdatedAt = isAdmin ? t.UpdatedAt : null,
                CreatedAt = isAdmin ? t.CreatedAt : null,
                DeletedById = isAdmin ? t.DeletedById : null,
                DeletedAt = isAdmin ? t.DeletedAt : null,
            })
            .ToListAsync(ct);

        return new PagedResponse<TypeDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task SoftDeleteAsync(Guid typeId, Guid actorId, CancellationToken ct)
    {
        var type = await _typeRepository.GetByIdAsync(typeId, cancellationToken: ct);

        if (type == null)
            throw new NotFoundException($"Type with id {typeId} not found");

        type.IsDeleted = true;
        type.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        type.DeletedById = actorId;

        await _typeRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task<TypeDetailDto> UpdateAsync(Guid actorId, UpdateTypeCommand request, CancellationToken ct)
    {
        var type = await _typeRepository.GetByIdAsync(request.Id, cancellationToken: ct);

        if (type == null)
            throw new NotFoundException($"Type with id {request.Id} not found");

        if (string.IsNullOrWhiteSpace(request.TypeName))
            throw new BadRequestException("Type Name or Code cannot be empty");

        type.TypeName = request.TypeName;

        type.UpdatedById = actorId;
        type.UpdatedAt = _dateTimeProvider.OffsetUtcNow;

        await _typeRepository.UnitOfWork.SaveChangesAsync(ct);

        return new TypeDetailDto
        {
            Id = type.Id,
            TypeName = type.TypeName,
            UpdatedAt = type.UpdatedAt,
            UpdatedById = type.UpdatedById
        };
    }
}
