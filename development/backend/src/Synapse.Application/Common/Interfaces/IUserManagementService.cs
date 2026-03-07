using Synapse.Application.Users.Dtos;

namespace Synapse.Application.Common.Interfaces;

/// <summary>
/// ユーザー管理操作のインターフェース。
/// ASP.NET Core Identity の UserManager を直接 Application 層に持ち込まないための抽象化。
/// 実装は Infrastructure 層（UserManagementService）に置く。
/// </summary>
public interface IUserManagementService
{
    Task<List<UserDto>> GetUsersAsync(CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(string id, CancellationToken ct = default);

    /// <returns>成功時は (UserId, null)、失敗時は (string.Empty, エラーメッセージ)</returns>
    Task<(string UserId, string? Error)> CreateUserAsync(
        string userName, string displayName, string password,
        IEnumerable<string> roles, CancellationToken ct = default);

    /// <returns>成功時は null、失敗時はエラーメッセージ</returns>
    Task<string?> UpdateUserAsync(
        string id, string displayName,
        IEnumerable<string> roles, CancellationToken ct = default);

    /// <returns>成功時は null、失敗時はエラーメッセージ</returns>
    Task<string?> DeleteUserAsync(string id, CancellationToken ct = default);
}
