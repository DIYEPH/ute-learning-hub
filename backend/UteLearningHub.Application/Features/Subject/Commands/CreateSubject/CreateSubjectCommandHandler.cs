using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using SubjectEntity = UteLearningHub.Domain.Entities.Subject;

namespace UteLearningHub.Application.Features.Subject.Commands.CreateSubject;

public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, SubjectDetailDto>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IMajorRepository _majorRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateSubjectCommandHandler(
        ISubjectRepository subjectRepository,
        IMajorRepository majorRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _subjectRepository = subjectRepository;
        _majorRepository = majorRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SubjectDetailDto> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create subjects");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate SubjectName and SubjectCode are not empty
        if (string.IsNullOrWhiteSpace(request.SubjectName))
            throw new BadRequestException("SubjectName cannot be empty");

        if (string.IsNullOrWhiteSpace(request.SubjectCode))
            throw new BadRequestException("SubjectCode cannot be empty");

        // ✅ Validate all MajorIds exist
        if (request.MajorIds.Any())
        {
            var existingMajorIds = await _majorRepository.GetQueryableSet()
                .Where(m => request.MajorIds.Contains(m.Id) && !m.IsDeleted)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);

            var invalidMajorIds = request.MajorIds.Except(existingMajorIds).ToList();
            if (invalidMajorIds.Any())
                throw new BadRequestException($"Major IDs not found: {string.Join(", ", invalidMajorIds)}");
        }

        // Check if subject name or code already exists
        var existingSubject = await _subjectRepository.GetQueryableSet()
            .Where(s =>
                (s.SubjectName.ToLower() == request.SubjectName.ToLower() ||
                 s.SubjectCode.ToLower() == request.SubjectCode.ToLower())
                && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSubject != null)
        {
            if (existingSubject.SubjectName.ToLower() == request.SubjectName.ToLower())
                throw new BadRequestException($"Subject with name '{request.SubjectName}' already exists");
            if (existingSubject.SubjectCode.ToLower() == request.SubjectCode.ToLower())
                throw new BadRequestException($"Subject with code '{request.SubjectCode}' already exists");
        }

        // Create subject
        var subject = new SubjectEntity
        {
            Id = Guid.NewGuid(),
            SubjectName = request.SubjectName,
            SubjectCode = request.SubjectCode,
            ReviewStatus = ReviewStatus.Approved,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _subjectRepository.AddAsync(subject, cancellationToken);

        // Create SubjectMajor relationships
        await _subjectRepository.AddSubjectMajorRelationshipsAsync(subject.Id, request.MajorIds, cancellationToken);

        await _subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get majors for response
        var majors = new List<MajorDto>();
        if (request.MajorIds.Any())
        {
            majors = await _majorRepository.GetQueryableSet()
                .Include(m => m.Faculty)
                .Where(m => request.MajorIds.Contains(m.Id))
                .Select(m => new MajorDto
                {
                    Id = m.Id,
                    MajorName = m.MajorName,
                    MajorCode = m.MajorCode,
                    Faculty = m.Faculty != null ? new FacultyDto
                    {
                        Id = m.Faculty.Id,
                        FacultyName = m.Faculty.FacultyName,
                        FacultyCode = m.Faculty.FacultyCode,
                        Logo = m.Faculty.Logo
                    } : null
                })
                .ToListAsync(cancellationToken);
        }

        return new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectName = subject.SubjectName,
            SubjectCode = subject.SubjectCode,
            Majors = majors,
            DocumentCount = 0
        };
    }
}
