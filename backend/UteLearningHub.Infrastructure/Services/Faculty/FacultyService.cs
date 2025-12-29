using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;
using UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;
using UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;
using UteLearningHub.Application.Services.Faculty;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

using FacultyEntity = UteLearningHub.Domain.Entities.Faculty;

namespace UteLearningHub.Infrastructure.Services.Faculty;

public class FacultyService(IFacultyRepository facultyRepository, IDateTimeProvider dateTimeProvider) : IFacultyService
{
    private readonly IFacultyRepository _facultyRepository = facultyRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    public async Task<FacultyDetailDto> CreateAsync(Guid creatorId, CreateFacultyCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FacultyName))
            throw new BadRequestException("Name cannot be empty");

        var exists = await _facultyRepository
            .GetQueryableSet()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => EF.Functions.Like(f.FacultyName, request.FacultyName.Trim()), ct);

        if (exists != null)
            throw new BadRequestException($"Faculty with name '{request.FacultyName}' already exists");

        var faculty = new FacultyEntity
        {
            Id = Guid.NewGuid(),
            FacultyName = request.FacultyName,
            FacultyCode = request.FacultyCode ?? string.Empty,
            CreatedById = creatorId,
            CreatedAt = _dateTimeProvider.OffsetUtcNow,
        };

        _facultyRepository.Add(faculty);
        await _facultyRepository.UnitOfWork.SaveChangesAsync(ct);

        return new FacultyDetailDto
        {
            Id = faculty.Id,
            FacultyName = faculty.FacultyName,
            FacultyCode = faculty.FacultyCode
        };
    }

    public async Task<PagedResponse<FacultyDetailDto>> GetFacultiesAsync(GetFacultiesQuery request, bool isAdmin, CancellationToken ct)
    {
        var query = _facultyRepository.GetQueryableSet();

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
            query = query.Where(f => EF.Functions.Like(f.FacultyName, term) || EF.Functions.Like(f.FacultyCode, term));
        }

        query = query.OrderBy(a => a.FacultyName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(f => new FacultyDetailDto
            {
                Id = f.Id,
                FacultyName = f.FacultyName,
                FacultyCode = f.FacultyCode,
                Logo = f.Logo,
                MajorCount = f.Majors.Count(m => isAdmin || !m.IsDeleted),
                IsDeleted = isAdmin ? f.IsDeleted : null
            })
            .ToListAsync(ct);

        return new PagedResponse<FacultyDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<FacultyDetailDto> GetFacultyByIdAsync(Guid id, bool isAdmin, CancellationToken ct)
    {
        var query = _facultyRepository.GetQueryableSet()
            .Where(m => m.Id == id);

        if (isAdmin)
            query = query.IgnoreQueryFilters();

        var dto = await query
            .Select(f => new FacultyDetailDto
            {
                Id = f.Id,
                FacultyName = f.FacultyName,
                FacultyCode = f.FacultyCode,
                Logo = f.Logo,
                MajorCount = f.Majors.Count(m => isAdmin || !m.IsDeleted),
                IsDeleted = isAdmin ? f.IsDeleted : null
            })
            .FirstOrDefaultAsync(ct);

        if (dto == null)
            throw new NotFoundException($"Facuty with id {id} not found");

        return dto;
    }

    public async Task SoftDeleteAsync(Guid facultyId, Guid actorId, CancellationToken ct)
    {
        var faculty = await _facultyRepository.GetByIdAsync(facultyId, cancellationToken: ct);

        if (faculty == null)
            throw new NotFoundException($"Author with id {facultyId} not found");

        faculty.IsDeleted = true;
        faculty.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        faculty.DeletedById = actorId;

        await _facultyRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task<FacultyDetailDto> UpdateAsync(Guid actorId, UpdateFacultyCommand request, CancellationToken ct)
    {
        var faculty = await _facultyRepository.GetByIdAsync(request.Id, cancellationToken: ct);

        if (faculty == null)
            throw new NotFoundException($"Author with id {request.Id} not found");

        if (string.IsNullOrWhiteSpace(request.FacultyName) || string.IsNullOrWhiteSpace(request.FacultyCode))
            throw new BadRequestException("Faculty Name or Code cannot be empty");

        faculty.FacultyName = request.FacultyName;
        faculty.FacultyCode = request.FacultyCode;

        faculty.UpdatedById = actorId;
        faculty.UpdatedAt = _dateTimeProvider.OffsetUtcNow;

        await _facultyRepository.UnitOfWork.SaveChangesAsync(ct);

        return new FacultyDetailDto
        {
            Id = faculty.Id,
            FacultyName = faculty.FacultyName,
            FacultyCode = faculty.FacultyCode
        };
    }
}
