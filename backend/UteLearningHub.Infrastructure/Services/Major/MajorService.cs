using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Major.Commands.CreateMajor;
using UteLearningHub.Application.Features.Major.Commands.UpdateMajor;
using UteLearningHub.Application.Features.Major.Queries.GetMajors;
using UteLearningHub.Application.Services.Major;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

using MajorEntity = UteLearningHub.Domain.Entities.Major;

namespace UteLearningHub.Infrastructure.Services.Major;

public class MajorService(IMajorRepository majorRepository, IFacultyRepository facultyRepository, IDateTimeProvider dateTimeProvider) : IMajorService
{
    private readonly IMajorRepository _majorRepository = majorRepository;
    private readonly IFacultyRepository _facultyRepository = facultyRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    public async Task<MajorDetailDto> CreateAsync(Guid creatorId, CreateMajorCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.MajorName) || string.IsNullOrWhiteSpace(request.MajorCode))
            throw new BadRequestException("Name or code cannot be empty");

        var faculty = await _facultyRepository
            .GetQueryableSet()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == request.FacultyId, ct);

        if (faculty == null)
            throw new NotFoundException($"Faculty with id {request.FacultyId} not found");

        if (faculty.IsDeleted)
            throw new BadRequestException("Faculty has been deleted");

        var exist = await _majorRepository
            .GetQueryableSet()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => EF.Functions.Like(m.MajorName, request.MajorName.Trim()) || EF.Functions.Like(m.MajorCode, request.MajorCode.Trim()), ct);

        if (exist != null)
            throw new BadRequestException($"Major with name '{request.MajorName}' or code '{request.MajorCode}' already exists");

        var major = new MajorEntity
        {
            Id = Guid.NewGuid(),
            FacultyId = request.FacultyId,
            MajorName = request.MajorName,
            MajorCode = request.MajorCode,
            CreatedById = creatorId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        _majorRepository.Add(major);
        await _facultyRepository.UnitOfWork.SaveChangesAsync(ct);

        return new MajorDetailDto
        {
            Id = major.Id,
            MajorName = major.MajorName,
            MajorCode = major.MajorCode,
            FacultyId = major.FacultyId,
            FacultyName = faculty.FacultyName,
            FacultyCode = faculty.FacultyCode
        };
    }

    public async Task<MajorDetailDto> GetMajorByIdAsync(Guid id, bool isAdmin, CancellationToken ct)
    {
        var query = _majorRepository.GetQueryableSet()
            .Where(m => m.Id == id);

        if (isAdmin)
            query = query.IgnoreQueryFilters();

        var dto = await query
            .Select(m => new MajorDetailDto
            {
                Id = m.Id,
                MajorName = m.MajorName,
                MajorCode = m.MajorCode,
                FacultyId = m.FacultyId,
                FacultyName = m.Faculty.FacultyName,
                FacultyCode = m.Faculty.FacultyCode,
                SubjectCount = m.SubjectMajors.Count(sm => isAdmin || !sm.Subject.IsDeleted),
                IsDeleted = isAdmin ? m.IsDeleted : null
            })
            .FirstOrDefaultAsync(ct);

        if (dto == null)
            throw new NotFoundException($"Major with id {id} not found");

        return dto;
    }

    public async Task<PagedResponse<MajorDetailDto>> GetMajorsAsync(GetMajorsQuery request, bool isAdmin, CancellationToken ct)
    {
        var query = _majorRepository.GetQueryableSet();

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
            query = query.Where(f => EF.Functions.Like(f.MajorName, term) || EF.Functions.Like(f.MajorCode, term));
        }

        if (request.FacultyId.HasValue)
            query = query.Where(m => m.FacultyId == request.FacultyId.Value);

        query = query.OrderBy(a => a.MajorName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(m => new MajorDetailDto
            {
                Id = m.Id,
                MajorName = m.MajorName,
                MajorCode = m.MajorCode,
                FacultyId = m.FacultyId,
                FacultyName = m.Faculty.FacultyName,
                FacultyCode = m.Faculty.FacultyCode,
                SubjectCount = m.SubjectMajors.Count(sm => isAdmin || !sm.Subject.IsDeleted),
                IsDeleted = isAdmin ? m.IsDeleted : null
            }).ToListAsync(ct);

        return new PagedResponse<MajorDetailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task SoftDeleteAsync(Guid majorId, Guid actorId, CancellationToken ct)
    {
        var major = await _majorRepository.GetByIdAsync(majorId, cancellationToken: ct);

        if (major == null)
            throw new NotFoundException($"Author with id {majorId} not found");

        major.IsDeleted = true;
        major.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        major.DeletedById = actorId;

        await _facultyRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task<MajorDetailDto> UpdateAsync(Guid actorId, UpdateMajorCommand request, CancellationToken ct)
    {
        var major = await _majorRepository.GetByIdAsync(request.Id, cancellationToken: ct);

        if (major == null)
            throw new NotFoundException($"Author with id {request.Id} not found");

        if (string.IsNullOrWhiteSpace(request.MajorName) || string.IsNullOrWhiteSpace(request.MajorCode))
            throw new BadRequestException("Faculty Name or Code cannot be empty");

        major.MajorName = request.MajorName;
        major.MajorCode = request.MajorCode;

        major.UpdatedById = actorId;
        major.UpdatedAt = _dateTimeProvider.OffsetUtcNow;

        await _majorRepository.UnitOfWork.SaveChangesAsync(ct);

        return new MajorDetailDto
        {
            Id = major.Id,
            MajorName = major.MajorName,
            MajorCode = major.MajorCode,
            FacultyId = major.FacultyId
        };
    }
}
