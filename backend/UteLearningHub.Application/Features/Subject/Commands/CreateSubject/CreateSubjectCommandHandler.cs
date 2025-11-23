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

        // Validate MajorId exists
        var major = await _majorRepository.GetQueryableSet()
            .Include(m => m.Faculty)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MajorId && !m.IsDeleted, cancellationToken);
        
        if (major == null)
            throw new NotFoundException($"Major with id {request.MajorId} not found");

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
            MajorId = request.MajorId,
            SubjectName = request.SubjectName,
            SubjectCode = request.SubjectCode,
            ReviewStatus = ReviewStatus.Approved, // Admin tạo nhưng cần review
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _subjectRepository.AddAsync(subject, cancellationToken);
        await _subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return new SubjectDetailDto
        {
            Id = subject.Id,
            SubjectName = subject.SubjectName,
            SubjectCode = subject.SubjectCode,
            Major = new MajorDto
            {
                Id = major.Id,
                MajorName = major.MajorName,
                MajorCode = major.MajorCode,
                Faculty = major.Faculty != null ? new FacultyDto
                {
                    Id = major.Faculty.Id,
                    FacultyName = major.Faculty.FacultyName,
                    FacultyCode = major.Faculty.FacultyCode
                } : null
            },
            DocumentCount = 0
        };
    }
}
