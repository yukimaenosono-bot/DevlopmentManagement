using MediatR;
using Synapse.Application.Common.Interfaces;

namespace Synapse.Application.Users.Commands;

/// <summary>
/// ユーザーを新規作成するコマンド。戻り値は生成されたユーザーの ID。
/// ロール名は設計書のロール定義（`detailed-design/08_バックエンド設計方針.md` セクション8）に従う。
/// </summary>
public record CreateUserCommand(
    string UserName,
    string DisplayName,
    string Password,
    IEnumerable<string> Roles
) : IRequest<string>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IUserManagementService _userManagement;

    public CreateUserCommandHandler(IUserManagementService userManagement)
    {
        _userManagement = userManagement;
    }

    public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (userId, error) = await _userManagement.CreateUserAsync(
            request.UserName, request.DisplayName, request.Password,
            request.Roles, cancellationToken);

        if (error is not null)
            throw new InvalidOperationException(error);

        return userId;
    }
}
