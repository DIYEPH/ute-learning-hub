using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;

public class UpdateFacultyCommandHandler : IRequestHandler<UpdateFacultyCommand, FacultyDetailDto>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateFacultyCommandHandler(
        IFacultyRepository facultyRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _facultyRepository = facultyRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<FacultyDetailDto> Handle(UpdateFacultyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update faculties");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate FacultyName and FacultyCode are not empty
        if (string.IsNullOrWhiteSpace(request.FacultyName))
            throw new BadRequestException("FacultyName cannot be empty");

        if (string.IsNullOrWhiteSpace(request.FacultyCode))
            throw new BadRequestException("FacultyCode cannot be empty");

        var faculty = await _facultyRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (faculty == null || faculty.IsDeleted)
            throw new NotFoundException($"Faculty with id {request.Id} not found");

        // Check if faculty name or code already exists (excluding current faculty)
        var existingFaculty = await _facultyRepository.GetQueryableSet()
            .Where(f => f.Id != request.Id 
                && (f.FacultyName.ToLower() == request.FacultyName.ToLower() || 
                    f.FacultyCode.ToLower() == request.FacultyCode.ToLower()) 
                && !f.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingFaculty != null)
        {
            if (existingFaculty.FacultyName.ToLower() == request.FacultyName.ToLower())
                throw new BadRequestException($"Faculty with name '{request.FacultyName}' already exists");
            if (existingFaculty.FacultyCode.ToLower() == request.FacultyCode.ToLower())
                throw new BadRequestException($"Faculty with code '{request.FacultyCode}' already exists");
        }

        // Update faculty
        faculty.FacultyName = request.FacultyName;
        faculty.FacultyCode = request.FacultyCode;
        if (request.Logo != null)
        {
            faculty.Logo = request.Logo;
        }
        faculty.UpdatedById = userId;
        faculty.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _facultyRepository.UpdateAsync(faculty, cancellationToken);
        await _facultyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get major count
        var majorCount = await _facultyRepository.GetQueryableSet()
            .Where(f => f.Id == request.Id)
            .Select(f => f.Majors.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new FacultyDetailDto
        {
            Id = faculty.Id,
            FacultyName = faculty.FacultyName,
            FacultyCode = faculty.FacultyCode,
            Logo = faculty.Logo,
            MajorCount = majorCount
        };
    }
}

