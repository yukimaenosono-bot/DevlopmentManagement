using Microsoft.AspNetCore.Identity;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Users.Dtos;

namespace Synapse.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity の UserManager を使ったユーザー管理の実装。
/// UserManager は Infrastructure に属するため、Application 層には IUserManagementService として公開する。
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly UserManager<AppUser> _userManager;

    public UserManagementService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserDto>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = _userManager.Users.ToList();

        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(ToDto(user, roles));
        }

        return result;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return ToDto(user, roles);
    }

    public async Task<(string UserId, string? Error)> CreateUserAsync(
        string userName, string displayName, string password,
        IEnumerable<string> roles, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = userName,
            DisplayName = displayName,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            // Identity の ValidationError をまとめて1つのメッセージにする
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (string.Empty, errors);
        }

        // ロールを付与する（存在しないロール名は無視せずエラーにする）
        var addRolesResult = await _userManager.AddToRolesAsync(user, roles);
        if (!addRolesResult.Succeeded)
        {
            // ロール付与に失敗したらユーザー自体を削除してロールバックに近い動作をする
            await _userManager.DeleteAsync(user);
            var errors = string.Join("; ", addRolesResult.Errors.Select(e => e.Description));
            return (string.Empty, errors);
        }

        return (user.Id, null);
    }

    public async Task<string?> UpdateUserAsync(
        string id, string displayName,
        IEnumerable<string> roles, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return "NotFound";

        user.DisplayName = displayName;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return string.Join("; ", updateResult.Errors.Select(e => e.Description));

        // 既存ロールをすべて剥がして付け直す（差分管理より単純で安全）
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, roles);

        return null;
    }

    public async Task<string?> DeleteUserAsync(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return "NotFound";

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return string.Join("; ", result.Errors.Select(e => e.Description));

        return null;
    }

    private static UserDto ToDto(AppUser user, IList<string> roles)
        => new(user.Id, user.UserName ?? string.Empty, user.DisplayName, roles.ToList());
}
