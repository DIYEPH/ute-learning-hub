using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Commands.UpdateProfile;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.User;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserService(
        ApplicationDbContext dbContext, 
        IIdentityService identityService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.FindByIdAsync(userId);

        if (user is null)
            return null;

        var roles = await _identityService.GetRolesAsync(userId);

        var appUser = await _dbContext.Users
            .Include(u => u.Major)
                .ThenInclude(m => m.Faculty)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (appUser is null)
            return null;

        MajorDto? majorDto = null;

        if (appUser.MajorId.HasValue && appUser.Major != null)
            majorDto = new MajorDto
            {
                Id = appUser.Major.Id,
                MajorName = appUser.Major.MajorName,
                MajorCode = appUser.Major.MajorCode,
                Faculty = appUser.Major.Faculty != null
                    ? new FacultyDto
                    {
                        Id = appUser.Major.Faculty.Id,
                        FacultyName = appUser.Major.Faculty.FacultyName,
                        FacultyCode = appUser.Major.Faculty.FacultyCode
                    }
                    : null
            };

        return new ProfileDto
        {
            Id = userId,
            Username = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            EmailConfirmed = user.EmailConfirmed,
            AvatarUrl = user.AvatarUrl,
            Introduction = user.Introduction,
            TrustScore = appUser.TrustScore,
            TrustLevel = appUser.TrustLever.ToString(),
            Gender = appUser.Gender.ToString(),
            Roles = roles.ToList(),
            Major = majorDto,
            CreatedAt = appUser.CreatedAt
        };
    }

    public async Task<TrustLever?> GetTrustLevelAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var appUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return appUser?.TrustLever;
    }

    public async Task<ProfileDto> UpdateProfileAsync(
        Guid userId, 
        UpdateProfileRequest request, 
        CancellationToken cancellationToken = default)
    {
        var appUser = await _dbContext.Users
            .Include(u => u.Major)
                .ThenInclude(m => m.Faculty)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (appUser == null)
            throw new NotFoundException("User not found");

        // Update properties if provided
        if (!string.IsNullOrWhiteSpace(request.FullName))
            appUser.FullName = request.FullName;

        if (!string.IsNullOrWhiteSpace(request.Introduction))
            appUser.Introduction = request.Introduction;

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            appUser.AvatarUrl = request.AvatarUrl;

        if (request.MajorId.HasValue)
        {
            var major = await _dbContext.Majors
                .FirstOrDefaultAsync(m => m.Id == request.MajorId.Value && !m.IsDeleted, cancellationToken);
            
            if (major == null)
                throw new NotFoundException($"Major with id {request.MajorId.Value} not found");
            
            appUser.MajorId = request.MajorId.Value;
        }

        if (request.Gender.HasValue)
            appUser.Gender = request.Gender.Value;

        appUser.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Return updated profile
        return await GetProfileAsync(userId, cancellationToken) 
            ?? throw new NotFoundException("User not found");
    }
}
