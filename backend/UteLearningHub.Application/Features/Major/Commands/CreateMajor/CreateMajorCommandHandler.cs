using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using MajorEntity = UteLearningHub.Domain.Entities.Major;

namespace UteLearningHub.Application.Features.Major.Commands.CreateMajor;

public class CreateMajorCommandHandler : IRequestHandler<CreateMajorCommand, MajorDetailDto>
{
    private readonly IMajorRepository _majorRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateMajorCommandHandler(
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

    public async Task<MajorDetailDto> Handle(CreateMajorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create majors");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate MajorName and MajorCode are not empty
        if (string.IsNullOrWhiteSpace(request.MajorName))
            throw new BadRequestException("MajorName cannot be empty");

        if (string.IsNullOrWhiteSpace(request.MajorCode))
            throw new BadRequestException("MajorCode cannot be empty");

        // Validate FacultyId exists
        var faculty = await _facultyRepository.GetByIdAsync(request.FacultyId, disableTracking: true, cancellationToken);
        if (faculty == null || faculty.IsDeleted)
            throw new NotFoundException($"Faculty with id {request.FacultyId} not found");

        // Check if major name or code already exists
        var existingMajor = await _majorRepository.GetQueryableSet()
            .Where(m =>
                (m.MajorName.ToLower() == request.MajorName.ToLower() ||
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

        // Create major
        var major = new MajorEntity
        {
            Id = Guid.NewGuid(),
            FacultyId = request.FacultyId,
            MajorName = request.MajorName,
            MajorCode = request.MajorCode,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _majorRepository.AddAsync(major, cancellationToken);
        await _majorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get faculty info for response
        var facultyInfo = await _facultyRepository.GetByIdAsync(request.FacultyId, disableTracking: true, cancellationToken);

        return new MajorDetailDto
        {
            Id = major.Id,
            MajorName = major.MajorName,
            MajorCode = major.MajorCode,
            Faculty = new FacultyDto
            {
                Id = facultyInfo!.Id,
                FacultyName = facultyInfo.FacultyName,
                FacultyCode = facultyInfo.FacultyCode,
                Logo = facultyInfo.Logo
            },
            SubjectCount = 0
        };
    }
}
