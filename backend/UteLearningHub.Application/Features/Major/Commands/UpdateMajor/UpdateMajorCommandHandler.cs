using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Major.Commands.UpdateMajor;

public class UpdateMajorCommandHandler : IRequestHandler<UpdateMajorCommand, MajorDetailDto>
{
    private readonly IMajorRepository _majorRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateMajorCommandHandler(
        IMajorRepository majorRepository,
        IFacultyRepository facultyRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _majorRepository = majorRepository;
        _facultyRepository = facultyRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<MajorDetailDto> Handle(UpdateMajorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update majors");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate MajorName and MajorCode are not empty
        if (string.IsNullOrWhiteSpace(request.MajorName))
            throw new BadRequestException("MajorName cannot be empty");

        if (string.IsNullOrWhiteSpace(request.MajorCode))
            throw new BadRequestException("MajorCode cannot be empty");

        var major = await _majorRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (major == null || major.IsDeleted)
            throw new NotFoundException($"Major with id {request.Id} not found");

        // Validate FacultyId exists
        var faculty = await _facultyRepository.GetByIdAsync(request.FacultyId, disableTracking: true, cancellationToken);
        if (faculty == null || faculty.IsDeleted)
            throw new NotFoundException($"Faculty with id {request.FacultyId} not found");

        // Check if major name or code already exists (excluding current major)
        var existingMajor = await _majorRepository.GetQueryableSet()
            .Where(m => m.Id != request.Id 
                && (m.MajorName.ToLower() == request.MajorName.ToLower() || 
                    m.MajorCode.ToLower() == request.MajorCode.ToLower()) 
                && !m.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingMajor != null)
        {
            if (existingMajor.MajorName.ToLower() == request.MajorName.ToLower())
                throw new BadRequestException($"Major with name '{request.MajorName}' already exists");
            if (existingMajor.MajorCode.ToLower() == request.MajorCode.ToLower())
                throw new BadRequestException($"Major with code '{request.MajorCode}' already exists");
        }

        // Update major
        major.FacultyId = request.FacultyId;
        major.MajorName = request.MajorName;
        major.MajorCode = request.MajorCode;
        major.UpdatedById = userId;
        major.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _majorRepository.UpdateAsync(major, cancellationToken);
        await _majorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get faculty info and subject count
        var facultyInfo = await _facultyRepository.GetByIdAsync(request.FacultyId, disableTracking: true, cancellationToken);
        var subjectCount = await _majorRepository.GetQueryableSet()
            .Where(m => m.Id == request.Id)
            .Select(m => m.Subjects.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new MajorDetailDto
        {
            Id = major.Id,
            MajorName = major.MajorName,
            MajorCode = major.MajorCode,
            Faculty = new FacultyDto
            {
                Id = facultyInfo!.Id,
                FacultyName = facultyInfo.FacultyName,
                FacultyCode = facultyInfo.FacultyCode
            },
            SubjectCount = subjectCount
        };
    }
}
