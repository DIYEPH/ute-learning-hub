using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Subject.Commands.CreateSubject;
using UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;
using UteLearningHub.Application.Features.Subject.Queries.GetSubjects;
using UteLearningHub.Application.Services.Subject;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

using SubjectEntity = UteLearningHub.Domain.Entities.Subject;


namespace UteLearningHub.Infrastructure.Services.Subject;

public class SubjectService(IMajorRepository majorRepository, ISubjectRepository subjectRepository, IDateTimeProvider dateTimeProvider) : ISubjectService
{
    private readonly IMajorRepository _majorRepository = majorRepository;
    private readonly ISubjectRepository _subjectRepository = subjectRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    public async Task<SubjectDetailDto> CreateAsync(Guid creatorId, CreateSubjectCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SubjectName) || string.IsNullOrWhiteSpace(request.SubjectCode))
            throw new BadRequestException("Name or code cannot be empty");

        var majors = await _majorRepository
            .GetQueryableSet()
            .IgnoreQueryFilters()
            .Where(m => request.MajorIds.Contains(m.Id))
            .ToListAsync(ct);

        if (majors.Count != request.MajorIds.Count)
            throw new NotFoundException("One or more majors not found");

        if (majors.Any(m => m.IsDeleted))
            throw new BadRequestException("One or more majors have been deleted");

        var exist = await _subjectRepository
            .GetQueryableSet()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => EF.Functions.Like(m.SubjectName, request.SubjectName.Trim()) || EF.Functions.Like(m.SubjectCode, request.SubjectCode.Trim()), ct);

        if (exist != null)
            throw new BadRequestException($"Subject with name '{request.SubjectName}' or code '{request.SubjectCode}' already exists");

        var subject = new SubjectEntity
        {
            Id = Guid.NewGuid(),
            SubjectCode = request.SubjectCode.Trim(),
            SubjectName = request.SubjectName.Trim(),
            CreatedById = creatorId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        subject.SubjectMajors = majors.Select(m => new SubjectMajor
        {
            SubjectId = subject.Id,
            MajorId = m.Id
        }).ToList();

        _subjectRepository.Add(subject);
        await _subjectRepository.UnitOfWork.SaveChangesAsync(ct);

        return new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Majors = majors.Select(m => new MajorDto
            {
                Id = m.Id,
                MajorName = m.MajorName,
                MajorCode = m.MajorCode,
                FacultyName = m.Faculty?.FacultyName,
                FacultyCode = m.Faculty?.FacultyCode
            }).ToList(),
            CreatedById = subject.CreatedById,
            CreatedAt = _dateTimeProvider.OffsetNow
        };
    }

    public async Task<SubjectDetailDto> GetSubjectByIdAsync(Guid id, bool isAdmin, CancellationToken ct)
    {
        var query = _subjectRepository.GetQueryableSet();

        if (isAdmin)
            query = query.IgnoreQueryFilters();

        var subject = await query
            .Include(s => s.SubjectMajors)
                .ThenInclude(sm => sm.Major)
                    .ThenInclude(m => m.Faculty)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (subject == null)
            throw new NotFoundException($"Subject with id {id} not found");

        if (!isAdmin && subject.Status != ContentStatus.Approved)
            throw new NotFoundException($"Subject with id {id} not found");

        return new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectName = subject.SubjectName,
            SubjectCode = subject.SubjectCode,
            Majors = subject.SubjectMajors
                .Select(sm => new MajorDto
                {
                    Id = sm.Major.Id,
                    MajorName = sm.Major.MajorName,
                    MajorCode = sm.Major.MajorCode,
                    FacultyName = sm.Major.Faculty?.FacultyName,
                    FacultyCode = sm.Major.Faculty?.FacultyCode
                })
                .ToList(),
            DocumentCount = subject.Documents.Count(d => !d.IsDeleted),

            IsDeleted = subject.IsDeleted,
            CreatedById = subject.CreatedById,
            UpdatedById = subject.UpdatedById,
            DeletedById = subject.DeletedById,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt,
            DeletedAt = subject.DeletedAt
        };
    }


    public async Task<PagedResponse<SubjectDetailDto>> GetSubjectsAsync(GetSubjectsQuery request, bool isAdmin, CancellationToken ct)
    {
        var query = _subjectRepository.GetQueryableSet().AsNoTracking();

        if (isAdmin)
        {
            query = query.IgnoreQueryFilters();
            if (request.IsDeleted.HasValue)
                query = query.Where(s => s.IsDeleted == request.IsDeleted.Value);
        }

        if (!isAdmin)
            query = query.Where(s => s.Status == ContentStatus.Approved);

        if (request.MajorIds?.Any() == true)
        {
            query = query.Where(s =>
                s.SubjectMajors.Any(sm => request.MajorIds.Contains(sm.MajorId)));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(s => s.SubjectName.ToLower().Contains(term) || s.SubjectCode.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var subjects = await query
            .Include(s => s.SubjectMajors)
                .ThenInclude(sm => sm.Major)
                    .ThenInclude(m => m.Faculty)
            .OrderBy(s => s.SubjectName)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(s => new SubjectDetailDto
            {
                Id = s.Id,
                SubjectName = s.SubjectName,
                SubjectCode = s.SubjectCode,
                Majors = s.SubjectMajors.Select(sm => new MajorDto
                {
                    Id = sm.Major.Id,
                    MajorName = sm.Major.MajorName,
                    MajorCode = sm.Major.MajorCode,
                    FacultyName = sm.Major.Faculty.FacultyName,
                    FacultyCode = sm.Major.Faculty.FacultyCode
                }).ToList(),
                DocumentCount = s.Documents.Count(d => !d.IsDeleted),

                IsDeleted = s.IsDeleted,
                CreatedById = s.CreatedById,
                UpdatedById = s.UpdatedById,
                DeletedById = s.DeletedById,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                DeletedAt = s.DeletedAt
            })
            .ToListAsync(ct);

        return new PagedResponse<SubjectDetailDto>
        {
            Items = subjects,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task SoftDeleteAsync(Guid subjectId, Guid actorId, CancellationToken ct)
    {
        var subject = await _subjectRepository.GetByIdAsync(subjectId, cancellationToken: ct);

        if (subject == null)
            throw new NotFoundException($"Subject with id {subject} not found");

        subject.IsDeleted = true;
        subject.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        subject.DeletedById = actorId;

        await _subjectRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task<SubjectDetailDto> UpdateAsync(Guid actorId, UpdateSubjectCommand request, CancellationToken ct)
    {
        var subject = await _subjectRepository.GetByIdAsync(request.Id, cancellationToken: ct);

        if (subject == null)
            throw new NotFoundException($"Subject with id {request.Id} not found");

        if (string.IsNullOrWhiteSpace(request.SubjectName) ||
            string.IsNullOrWhiteSpace(request.SubjectCode))
            throw new BadRequestException("Subject name or code cannot be empty");

        subject.SubjectName = request.SubjectName.Trim();
        subject.SubjectCode = request.SubjectCode.Trim();
        subject.UpdatedById = actorId;
        subject.UpdatedAt = _dateTimeProvider.OffsetUtcNow;

        subject.SubjectMajors = request.MajorIds
        .Select(majorId => new SubjectMajor
        {
            SubjectId = subject.Id,
            MajorId = majorId
        })
        .ToList();

        await _subjectRepository.UnitOfWork.SaveChangesAsync(ct);

        // Reload majors with faculty info
        var majors = await _majorRepository
            .GetQueryableSet()
            .Include(m => m.Faculty)
            .Where(m => request.MajorIds.Contains(m.Id))
            .ToListAsync(ct);

        return new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectName = subject.SubjectName,
            SubjectCode = subject.SubjectCode,
            Majors = majors.Select(m => new MajorDto
            {
                Id = m.Id,
                MajorName = m.MajorName,
                MajorCode = m.MajorCode,
                FacultyName = m.Faculty?.FacultyName,
                FacultyCode = m.Faculty?.FacultyCode
            }).ToList()
        };
    }
}
