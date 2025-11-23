namespace UteLearningHub.Application.Services.Identity;

public interface IIdentityService
{
    Task<AppUserDto?> FindByEmailAsync(string email);
    Task<AppUserDto?> FindByUsernameAsync(string username);
    Task<AppUserDto?> FindByIdAsync(Guid userId);
    Task<AppUserDto?> FindByExternalLoginAsync(string loginProvider, string providerKey);
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateUserDto dto);
    Task<bool> AddExternalLoginAsync(Guid userId, ExternalLoginInfoDto loginInfo);
    Task<IList<string>> GetRolesAsync(Guid userId);
    Task<bool> AddToRoleAsync(Guid userId, string roleName);
    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
}
public record AppUserDto(
    Guid Id,
    string Email,
    string? UserName,
    string FullName,
    bool EmailConfirmed,
    string? AvatarUrl,
    Guid? MajorId,
    string? Introduction
);
public record CreateUserDto(
    string Email,
    string? UserName,
    string? FullName,
    bool EmailConfirmed,
    Guid? MajorId,
    string? Introduction,
    string? AvatarUrl
);
public record ExternalLoginInfoDto(
    string LoginProvider,
    string ProviderKey,
    string DisplayName
);