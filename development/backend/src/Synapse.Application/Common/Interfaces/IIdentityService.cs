namespace Synapse.Application.Common.Interfaces;

/// <summary>
/// 認証サービスのインターフェース。
/// Application層はこれを通してJWTトークンを取得する（実装はInfrastructure層）。
/// </summary>
public interface IIdentityService
{
    Task<(bool Success, string Token)> LoginAsync(string userName, string password);
}
