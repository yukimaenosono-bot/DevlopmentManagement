using Microsoft.AspNetCore.Identity;

namespace Synapse.Infrastructure.Identity;

/// <summary>
/// ASP.NET Identity のユーザーエンティティ。
/// IdentityUser を継承することでパスワードハッシュ・ロック等の機能を持つ。
/// </summary>
public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
