using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Events;
using UteLearningHub.Application.Features.User.Commands.UpdateUser;
using UteLearningHub.Application.Features.User.Queries.GetUsers;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Policies;
using UteLearningHub.Persistence;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Infrastructure.Services.User;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UserManager<AppUser> _userManager;
    private readonly IVectorMaintenanceService? _vectorMaintenanceService;
    private readonly IMediator _mediator;

    public UserService(
        ApplicationDbContext dbContext,
        IIdentityService identityService,
        IDateTimeProvider dateTimeProvider,
        UserManager<AppUser> userManager,
        IMediator mediator,
        IVectorMaintenanceService? vectorMaintenanceService = null)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _dateTimeProvider = dateTimeProvider;
        _userManager = userManager;
        _mediator = mediator;
        _vectorMaintenanceService = vectorMaintenanceService;
    }

    public async Task<TrustLever?> GetTrustLevelAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var appUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return appUser?.TrustLever;
    }
    public async Task<IList<Guid>> GetAllActiveUserIdsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Where(u => !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<Guid>> ValidateUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var userIdList = userIds.ToList();
        if (!userIdList.Any())
            return new List<Guid>();

        return await _dbContext.Users
            .Where(u => userIdList.Contains(u.Id) && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResponse<UserDto>> GetUsersAsync(GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .Include(u => u.Major!)
                .ThenInclude(m => m!.Faculty)
            .AsNoTracking();

        // Search by name, email, or username
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(searchTerm) ||
                u.Email!.ToLower().Contains(searchTerm) ||
                (u.UserName != null && u.UserName.ToLower().Contains(searchTerm)));
        }

        // Filter by MajorId
        if (request.MajorId.HasValue)
        {
            query = query.Where(u => u.MajorId == request.MajorId.Value);
        }

        // Filter by TrustLevel
        if (request.TrustLevel.HasValue)
        {
            query = query.Where(u => u.TrustLever == request.TrustLevel.Value);
        }

        // Filter by EmailConfirmed
        if (request.EmailConfirmed.HasValue)
        {
            query = query.Where(u => u.EmailConfirmed == request.EmailConfirmed.Value);
        }

        // Filter by IsDeleted
        if (request.IsDeleted.HasValue)
        {
            query = query.Where(u => u.IsDeleted == request.IsDeleted.Value);
        }
        else
        {
            // Default: only show non-deleted users
            query = query.Where(u => !u.IsDeleted);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "fullname" or "name" => request.SortDescending
                ? query.OrderByDescending(u => u.FullName)
                : query.OrderBy(u => u.FullName),
            "email" => request.SortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "trustscore" or "score" => request.SortDescending
                ? query.OrderByDescending(u => u.TrustScore)
                : query.OrderBy(u => u.TrustScore),
            "lastloginat" or "lastlogin" => request.SortDescending
                ? query.OrderByDescending(u => u.LastLoginAt)
                : query.OrderBy(u => u.LastLoginAt),
            "createdat" or "date" => request.SortDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt) // Default: newest first
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var users = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        // Get roles for each user
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Username = user.UserName,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                AvatarUrl = user.AvatarUrl,
                Introduction = user.Introduction,
                TrustScore = user.TrustScore,
                TrustLevel = user.TrustLever.ToString(),
                Gender = user.Gender,
                IsSuggest = user.IsSuggest,
                Roles = roles.ToList(),
                Major = user.Major != null ? new MajorDetailDto
                {
                    Id = user.Major.Id,
                    MajorName = user.Major.MajorName,
                    MajorCode = user.Major.MajorCode
                } : null,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                IsDeleted = user.IsDeleted,
                DeletedAt = user.DeletedAt,
                DeletedById = user.DeletedById,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            });
        }

        return new PagedResponse<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Major!)
                .ThenInclude(m => m!.Faculty)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            Username = user.UserName,
            FullName = user.FullName,
            EmailConfirmed = user.EmailConfirmed,
            AvatarUrl = user.AvatarUrl,
            Introduction = user.Introduction,
            TrustScore = user.TrustScore,
            TrustLevel = user.TrustLever.ToString(),
            Gender = user.Gender,
            IsSuggest = user.IsSuggest,
            Roles = roles.ToList(),
            Major = user.Major != null ? new MajorDetailDto
            {
                Id = user.Major.Id,
                MajorName = user.Major.MajorName,
                MajorCode = user.Major.MajorCode
            } : null,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            IsDeleted = user.IsDeleted,
            DeletedAt = user.DeletedAt,
            DeletedById = user.DeletedById,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd
        };
    }

    public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Major!)
                .ThenInclude(m => m!.Faculty)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with id {userId} not found");

        // Update properties if provided
        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.Username))
            user.UserName = request.Username;

        if (!string.IsNullOrWhiteSpace(request.Introduction))
            user.Introduction = request.Introduction;

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            user.AvatarUrl = request.AvatarUrl;

        if (request.MajorId.HasValue)
        {
            var major = await _dbContext.Majors
                .FirstOrDefaultAsync(m => m.Id == request.MajorId.Value && !m.IsDeleted, cancellationToken);

            if (major == null)
                throw new NotFoundException($"Major with id {request.MajorId.Value} not found");

            user.MajorId = request.MajorId.Value;
        }

        if (request.Gender.HasValue)
            user.Gender = request.Gender.Value;

        if (request.EmailConfirmed.HasValue)
            user.EmailConfirmed = request.EmailConfirmed.Value;

        user.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Update roles if provided
        if (request.Roles != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, request.Roles);
        }

        // Return updated user
        return await GetUserByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found");
    }

    public async Task BanUserAsync(Guid userId, DateTimeOffset? banUntil, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException($"User with id {userId} not found");

        user.LockoutEnabled = true;
        user.LockoutEnd = banUntil ?? DateTimeOffset.UtcNow.AddYears(100); // Ban vĩnh viễn nếu null

        await _userManager.UpdateAsync(user);
    }

    public async Task UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException($"User with id {userId} not found");

        user.LockoutEnabled = false;
        user.LockoutEnd = null;

        await _userManager.UpdateAsync(user);
    }

    public async Task<UserDto> UpdateTrustScoreAsync(Guid userId, int trustScore, string? reason, Guid? entityId = null, TrustEntityType? entityType = null, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with id {userId} not found");

        var oldTrustScore = user.TrustScore;
        var oldTrustLevel = user.TrustLever;

        user.TrustScore = trustScore;
        user.UpdatedAt = _dateTimeProvider.OffsetNow;

        if (oldTrustLevel != TrustLever.Master)
            user.TrustLever = TrustLevelPolicy.Calculate(trustScore);

        if (!string.IsNullOrWhiteSpace(reason))
        {
            var trustHistory = new UserTrustHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Score = trustScore - oldTrustScore, // Delta score
                OldScore = oldTrustScore,
                NewScore = trustScore,
                Reason = reason,
                EntityId = entityId,
                EntityType = entityType,
                CreatedAt = _dateTimeProvider.OffsetNow
            };
            _dbContext.UserTrustHistories.Add(trustHistory);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var newTrustLevel = user.TrustLever;

        if (newTrustLevel > oldTrustLevel)
        {
            await _mediator.Publish(new TrustLevelChangedEvent
            {
                UserId = userId,
                OldLevel = oldTrustLevel,
                NewLevel = newTrustLevel,
                NewScore = trustScore,
                ScoreDelta = trustScore - oldTrustScore
            }, cancellationToken);
        }

        // Return updated user
        return await GetUserByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found");
    }
    public async Task<IList<UserTrustHistoryDto>> GetUserTrustHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var histories = await _dbContext.UserTrustHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .AsNoTracking()
            .Select(h => new UserTrustHistoryDto
            {
                Id = h.Id,
                UserId = h.UserId,
                Score = h.Score,
                OldScore = h.OldScore,
                NewScore = h.NewScore,
                Reason = h.Reason,
                EntityId = h.EntityId,
                EntityType = h.EntityType.HasValue ? (int)h.EntityType : null,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return histories;
    }
}
