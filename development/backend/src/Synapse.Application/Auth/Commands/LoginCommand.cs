using MediatR;

namespace Synapse.Application.Auth.Commands;

/// <summary>
/// ログインコマンド。MediatR経由でLoginCommandHandlerに渡される。
/// </summary>
public record LoginCommand(string UserName, string Password) : IRequest<LoginResult>;

/// <summary>
/// ログイン成功時に返すJWTトークン。
/// </summary>
public record LoginResult(string Token);
