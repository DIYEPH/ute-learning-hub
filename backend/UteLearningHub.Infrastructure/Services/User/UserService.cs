using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Queries.GetProfile;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.User;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;


    public UserService(ApplicationDbContext dbContext, IIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<GetProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
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

        return new GetProfileResponse
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
}
