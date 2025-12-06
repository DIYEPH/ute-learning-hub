using Microsoft.AspNetCore.Identity;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Infrastructure.Services.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    public IdentityService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task<AppUserDto?> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null ? null : MapToDto(user);
    }

    public async Task<AppUserDto?> FindByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user == null ? null : MapToDto(user);
    }
    public async Task<AppUserDto?> FindByExternalLoginAsync(string loginProvider, string providerKey)
    {
        var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
        return user == null ? null : MapToDto(user);
    }
    public async Task<(bool Succeeded, Guid UserId, IEnumerable<string> Errors)> CreateUserAsync(CreateUserDto dto)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = dto.EmailConfirmed,
            FullName = dto.FullName ?? string.Empty,
            MajorId = dto.MajorId,
            Introduction = dto.Introduction ?? string.Empty,
            AvatarUrl = dto.AvatarUrl ?? string.Empty,
            TrustScore = 0
        };

        var result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Student");
            return (true, user.Id, Enumerable.Empty<string>());
        }

        return (false, Guid.Empty, result.Errors.Select(e => e.Description));
    }
    public async Task<bool> AddExternalLoginAsync(Guid userId, ExternalLoginInfoDto loginInfo)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.AddLoginAsync(user, new UserLoginInfo(
            loginInfo.LoginProvider,
            loginInfo.ProviderKey,
            loginInfo.DisplayName
        ));

        return result.Succeeded;
    }
    public async Task<AppUserDto?> FindByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        return user == null ? null : MapToDto(user);
    }


    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        return await _userManager.CheckPasswordAsync(user, password);
    }
    public async Task<IList<string>> GetRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return [];

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AddToRoleAsync(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var result = await _userManager.AddToRoleAsync(user, roleName);

        return result.Succeeded;
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return;

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUsernameAsync(Guid userId, string newUsername)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return (false, new[] { "User not found" });

        // Check username đã tồn tại chưa
        var existingUser = await _userManager.FindByNameAsync(newUsername);
        if (existingUser != null && existingUser.Id != userId)
            return (false, new[] { "Username already exists" });

        user.UserName = newUsername;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return (true, Enumerable.Empty<string>());

        return (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
            return (false, new[] { "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        
        if (result.Succeeded)
            return (true, Enumerable.Empty<string>());

        return (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.IsDeleted)
            return (false, new[] { "User not found" });

        // Decode token từ URL
        var decodedToken = Uri.UnescapeDataString(token);

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        
        if (result.Succeeded)
            return (true, Enumerable.Empty<string>());

        return (false, result.Errors.Select(e => e.Description));
    }

    public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new Exception("User not found");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }
    private static AppUserDto MapToDto(AppUser user) => new(
        user.Id,
        user.Email!,
        user.UserName,
        user.FullName,
        user.EmailConfirmed,
        user.AvatarUrl,
        user.MajorId,
        user.Introduction
    );
}
