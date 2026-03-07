namespace Synapse.Application.Users.Dtos;

/// <summary>
/// ユーザー情報の参照用 DTO。
/// パスワードは含まない。フロントエンドの型定義（UserDto）と 1:1 で対応させること。
/// </summary>
public record UserDto(
    string Id,
    string UserName,
    string DisplayName,
    IReadOnlyList<string> Roles
);
