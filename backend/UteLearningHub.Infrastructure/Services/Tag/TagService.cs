using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Tag.Commands.UpdateTag;
using UteLearningHub.Application.Features.Tag.Queries.GetTags;
using UteLearningHub.Application.Services.Tag;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

using TagEntity = UteLearningHub.Domain.Entities.Tag;

namespace UteLearningHub.Infrastructure.Services.Tag;

public class TagService(ITagRepository tagRepository, IDateTimeProvider dateTimeProvider) : ITagService
{
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<TagDetailDto> CreateAsync(Guid creatorId, string tagName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new BadRequestException("Name cannot be empty");

        var exists = await _tagRepository
            .GetQueryableSet()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => EF.Functions.Like(f.TagName, tagName.Trim()), ct);

        if (exists != null)
            throw new BadRequestException($"Tag with name '{tagName}' already exists");

        var tag = new TagEntity
        {
            Id = Guid.NewGuid(),
            TagName = tagName,
            Status = ContentStatus.Approved,
            CreatedById = creatorId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        _tagRepository.Add(tag);
        await _tagRepository.UnitOfWork.SaveChangesAsync(ct);

        return new TagDetailDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            CreatedById = tag.CreatedById,
            CreatedAt = _dateTimeProvider.OffsetNow
        };
    }

    public async Task<TagDetailDto> GetTagByIdAsync(Guid id, bool isAdmin, CancellationToken ct)
    {
        var query = _tagRepository.GetQueryableSet()
            .Where(m => m.Id == id);

        if (isAdmin)
            query = query.IgnoreQueryFilters();

        var dto = await query
            .Select(t => new TagDetailDto
            {
                Id = t.Id,
                TagName = t.TagName,
                CreatedById = t.CreatedById,
                DocumentCount = t.DocumentTags.Count(dt => isAdmin || !dt.Document.IsDeleted),
                Status = t.Status,
                IsDeleted = isAdmin ? t.IsDeleted : null,
                UpdatedAt = isAdmin ? t.UpdatedAt : null,
                CreatedAt = isAdmin ? t.CreatedAt : null,
                DeletedById = isAdmin ? t.DeletedById : null,
                DeletedAt = isAdmin ? t.DeletedAt : null,
            })
            .FirstOrDefaultAsync(ct);

        if (dto == null)
            throw new NotFoundException($"Tag with id {id} not found");

        return dto;
    }

    public async Task<PagedResponse<TagDetailDto>> GetTagsAsync(GetTagsQuery request, bool isAdmin, CancellationToken ct)
    {
        var query = _tagRepository.GetQueryableSet();

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
            query = query.Where(f => EF.Functions.Like(f.TagName, term));
        }

        query = query.OrderBy(a => a.TagName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(t => new TagDetailDto
            {
                Id = t.Id,
                TagName = t.TagName,
                CreatedById = t.CreatedById,
                Status = t.Status,
                DocumentCount = t.DocumentTags.Count(dt => isAdmin || !dt.Document.IsDeleted),
                IsDeleted = isAdmin ? t.IsDeleted : null,
                UpdatedAt = isAdmin ? t.UpdatedAt : null,
                CreatedAt = isAdmin ? t.CreatedAt : null,
                DeletedById = isAdmin ? t.DeletedById : null,
                DeletedAt = isAdmin ? t.DeletedAt : null,
            }).ToListAsync(ct);

        return new PagedResponse<TagDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task SoftDeleteAsync(Guid tagId, Guid actorId, CancellationToken ct)
    {
        var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken: ct);

        if (tag == null)
            throw new NotFoundException($"Tag with id {tag} not found");

        tag.IsDeleted = true;
        tag.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        tag.DeletedById = actorId;

        await _tagRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task<TagDetailDto> UpdateAsync(Guid actorId, UpdateTagCommand request, CancellationToken ct)
    {
        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken: ct);

        if (tag == null)
            throw new NotFoundException($"Tag with id {request.Id} not found");

        if (string.IsNullOrWhiteSpace(request.TagName))
            throw new BadRequestException("TagName cannot be empty");

        tag.TagName = request.TagName;

        tag.UpdatedById = actorId;
        tag.UpdatedAt = _dateTimeProvider.OffsetUtcNow;

        await _tagRepository.UnitOfWork.SaveChangesAsync(ct);

        return new TagDetailDto
        {
            Id = tag.Id,
            TagName = tag.TagName,
            UpdatedAt = tag.UpdatedAt,
            UpdatedById = tag.UpdatedById
        };
    }
}
