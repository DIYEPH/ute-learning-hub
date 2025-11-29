using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;

public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, SubjectDetailDto>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly IMajorRepository _majorRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateSubjectCommandHandler(
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

    public async Task<SubjectDetailDto> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update subjects");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate SubjectName and SubjectCode are not empty
        if (string.IsNullOrWhiteSpace(request.SubjectName))
            throw new BadRequestException("SubjectName cannot be empty");

        if (string.IsNullOrWhiteSpace(request.SubjectCode))
            throw new BadRequestException("SubjectCode cannot be empty");

        var subject = await _subjectRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (subject == null || subject.IsDeleted)
            throw new NotFoundException($"Subject with id {request.Id} not found");

        // Validate all MajorIds exist
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

        // Check if subject name or code already exists (excluding current subject)
        var existingSubject = await _subjectRepository.GetQueryableSet()
            .Where(s => s.Id != request.Id 
                && (s.SubjectName.ToLower() == request.SubjectName.ToLower() || 
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

        // Update subject
        subject.SubjectName = request.SubjectName;
        subject.SubjectCode = request.SubjectCode;
        subject.UpdatedById = userId;
        subject.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _subjectRepository.UpdateAsync(subject, cancellationToken);

        // Update SubjectMajor relationships
        await _subjectRepository.UpdateSubjectMajorRelationshipsAsync(subject.Id, request.MajorIds, cancellationToken);

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
                        FacultyCode = m.Faculty.FacultyCode
                    } : null
                })
                .ToListAsync(cancellationToken);
        }

        // Get document count
        var documentCount = await _subjectRepository.GetQueryableSet()
            .Where(s => s.Id == request.Id)
            .Select(s => s.Documents.Count(d => !d.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken);

        return new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectName = subject.SubjectName,
            SubjectCode = subject.SubjectCode,
            Majors = majors,
            DocumentCount = documentCount
        };
    }
}