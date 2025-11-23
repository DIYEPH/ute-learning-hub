using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using FacultyEntity = UteLearningHub.Domain.Entities.Faculty;

namespace UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;

public class CreateFacultyCommandHandler : IRequestHandler<CreateFacultyCommand, FacultyDetailDto>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateFacultyCommandHandler(
        IFacultyRepository facultyRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _facultyRepository = facultyRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<FacultyDetailDto> Handle(CreateFacultyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create faculties");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate FacultyName and FacultyCode are not empty
        if (string.IsNullOrWhiteSpace(request.FacultyName))
            throw new BadRequestException("FacultyName cannot be empty");

        if (string.IsNullOrWhiteSpace(request.FacultyCode))
            throw new BadRequestException("FacultyCode cannot be empty");

        // Check if faculty name or code already exists
        var existingFaculty = await _facultyRepository.GetQueryableSet()
            .Where(f => 
                (f.FacultyName.ToLower() == request.FacultyName.ToLower() || 
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

        // Create faculty
        var faculty = new FacultyEntity
        {
            Id = Guid.NewGuid(),
            FacultyName = request.FacultyName,
            FacultyCode = request.FacultyCode,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _facultyRepository.AddAsync(faculty, cancellationToken);
        await _facultyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return new FacultyDetailDto
        {
            Id = faculty.Id,
            FacultyName = faculty.FacultyName,
            FacultyCode = faculty.FacultyCode,
            MajorCount = 0
        };
    }
}
