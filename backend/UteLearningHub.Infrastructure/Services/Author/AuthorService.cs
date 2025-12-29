using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Author.Commands.CreateAuthor;
using UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;
using UteLearningHub.Application.Features.Author.Queries.GetAuthors;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

using AuthorEntity = UteLearningHub.Domain.Entities.Author;

namespace UteLearningHub.Infrastructure.Services.Author;

public class AuthorService(IAuthorRepository authorRepository, IDateTimeProvider dateTimeProvider) : IAuthorService
{
    private readonly IAuthorRepository _authorRepository = authorRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<AuthorDetailDto> CreateAsync(Guid creatorId, CreateAuthorCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new BadRequestException("FullName cannot be empty");

        var author = new AuthorEntity
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Description = request.Description ?? string.Empty,
            Status = ContentStatus.Approved,
            CreatedById = creatorId,
            CreatedAt = _dateTimeProvider.OffsetUtcNow,
        };

        _authorRepository.Add(author);
        await _authorRepository.UnitOfWork.SaveChangesAsync(ct);

        return new AuthorDetailDto
        {
            Id = author.Id,
            FullName = author.FullName,
            Description = author.Description
        };
    }

    public async Task<PagedResponse<AuthorDetailDto>> GetAuthorsAsync(GetAuthorsQuery request, bool isAdmin, CancellationToken ct)
    {
        var query = _authorRepository.GetQueryableSet();

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
            query = query.Where(f => EF.Functions.Like(f.FullName, term));
        }

        if (!isAdmin)
            query = query.Where(a => a.Status == ContentStatus.Approved);

        query = query.OrderBy(a => a.FullName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(a => new AuthorDetailDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Description = a.Description,
                DocumentCount = a.DocumentAuthors.Count(da => isAdmin || !da.Document.IsDeleted),
                CreatedById = a.CreatedById,
                IsDeleted = isAdmin ? a.IsDeleted : null
            }).ToListAsync(ct);

        return new PagedResponse<AuthorDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<AuthorDetailDto> GetAuthorByIdAsync(Guid id, bool isAdmin, CancellationToken ct)
    {
        var query = _authorRepository.GetQueryableSet()
            .Where(m => m.Id == id);

        if (isAdmin)
            query = query.IgnoreQueryFilters();

        var dto = await query
            .Select(a => new AuthorDetailDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Description = a.Description,
                CreatedById = a.CreatedById,
                DocumentCount = a.DocumentAuthors.Count(da => isAdmin || !da.Document.IsDeleted),
                IsDeleted = isAdmin ? a.IsDeleted : null
            })
            .FirstOrDefaultAsync(ct);

        if (dto == null)
            throw new NotFoundException($"Author with id {id} not found");

        return dto;
    }

    public async Task SoftDeleteAsync(Guid authorId, Guid actorId, CancellationToken ct)
    {
        var author = await _authorRepository.GetByIdAsync(authorId, cancellationToken: ct);

        if (author == null)
            throw new NotFoundException($"Author with id {authorId} not found");

        author.IsDeleted = true;
        author.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        author.DeletedById = actorId;

        await _authorRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task<AuthorDetailDto> UpdateAsync(Guid actorId, UpdateAuthorCommand request, CancellationToken ct)
    {
        var author = await _authorRepository.GetByIdAsync(request.Id, cancellationToken: ct);

        if (author == null)
            throw new NotFoundException($"Author with id {request.Id} not found");

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new BadRequestException("FullName cannot be empty");

        author.FullName = request.FullName;
        author.Description = request.Description ?? string.Empty;

        author.UpdatedById = actorId;
        author.UpdatedAt = _dateTimeProvider.OffsetUtcNow;

        await _authorRepository.UnitOfWork.SaveChangesAsync(ct);

        return new AuthorDetailDto
        {
            Id = author.Id,
            FullName = author.FullName,
            Description = author.Description
        };
    }
}
