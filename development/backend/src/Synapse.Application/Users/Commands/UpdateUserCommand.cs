using MediatR;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Users.Commands;

/// <summary>
/// ユーザー情報を更新するコマンド。
/// UserName（ログイン名）の変更は別途パスワードリセットフロー等と組み合わせる必要があるため、
/// ここでは DisplayName とロールの変更のみを扱う。
/// </summary>
public record UpdateUserCommand(
    string Id,
    string DisplayName,
    IEnumerable<string> Roles
) : IRequest;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserManagementService _userManagement;

    public UpdateUserCommandHandler(IUserManagementService userManagement)
    {
        _userManagement = userManagement;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var error = await _userManagement.UpdateUserAsync(
            request.Id, request.DisplayName, request.Roles, cancellationToken);

        if (error == "NotFound")
            throw new NotFoundException("User", request.Id);

        if (error is not null)
            throw new InvalidOperationException(error);
    }
}
