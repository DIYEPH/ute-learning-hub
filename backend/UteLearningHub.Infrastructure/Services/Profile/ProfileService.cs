using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Commands.UpdateProfile;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Profile;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Policies;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Profile;

public class ProfileService(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, IIdentityService identityService) : IProfileService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IIdentityService _identityService = identityService;

    public async Task<ProfileDetailDto> GetProfileByIdAsync(Guid? userId, bool isAdmin, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.UserName,
                u.FullName,
                u.AvatarUrl,
                u.EmailConfirmed,
                u.Introduction,
                u.Gender,
                u.TrustScore,
                u.MajorId,
                u.IsSuggest,
                u.CreatedAt,
                u.LockoutEnabled,
                u.LockoutEnd
            })
            .FirstOrDefaultAsync(ct);

        if (user == null)
            throw new NotFoundException("User not found");

        var roles = await _identityService.GetRolesAsync(user.Id);
        var isLocked = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

        return new ProfileDetailDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName,
            FullName = user.FullName,
            EmailConfirmed = user.EmailConfirmed,
            AvatarUrl = user.AvatarUrl,
            Introduction = user.Introduction,
            TrustScore = user.TrustScore,
            TrustLevel = TrustLevelPolicy.Calculate(user.TrustScore),
            Gender = user.Gender,
            Roles = roles.ToList(),
            MajorId = user.MajorId,
            IsSuggest = user.IsSuggest,
            CreatedAt = user.CreatedAt,

            LockoutEnd = isAdmin ? user.LockoutEnd : null,
            IsLocked = isAdmin ? isLocked : null
        };
    }

    public async Task<ProfileDetailDto> UpdateAsync(Guid actorId, UpdateProfileCommand request, CancellationToken ct = default)
    {
        var appUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id, ct);

        if (appUser == null)
            throw new NotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(request.Introduction))
            appUser.Introduction = request.Introduction;

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            appUser.AvatarUrl = request.AvatarUrl;

        if (request.MajorId.HasValue)
        {
            var majorExists = await _dbContext.Majors.AnyAsync(m => m.Id == request.MajorId.Value && !m.IsDeleted && !m.Faculty.IsDeleted, ct);

            if (!majorExists)
                throw new NotFoundException($"Major with id {request.MajorId.Value} not found");

            appUser.MajorId = request.MajorId.Value;
        }

        if (request.Gender.HasValue)
            appUser.Gender = request.Gender.Value;

        if (request.IsSuggest.HasValue)
            appUser.IsSuggest = request.IsSuggest.Value;

        appUser.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _dbContext.SaveChangesAsync(ct);

        return await GetProfileByIdAsync(appUser.Id, true, ct) ?? throw new NotFoundException("User not found");
    }

    public async Task<UserStatsDto> GetUserStatsAsync(Guid userId, CancellationToken ct = default)
    {
        var uploadsCount = await _dbContext.DocumentFiles
            .Where(d => d.CreatedById == userId)
            .CountAsync(ct);

        var upvotesCount = await _dbContext.DocumentReviews
            .Where(r => r.DocumentFile.CreatedById == userId && r.DocumentReviewType == DocumentReviewType.Useful)
            .CountAsync(ct);

        var commentsCount = await _dbContext.Comments
            .Where(c => c.DocumentFile.CreatedById == userId)
            .CountAsync(ct);

        return new UserStatsDto
        {
            Uploads = uploadsCount,
            Upvotes = upvotesCount,
            Comments = commentsCount
        };
    }
}
