using MediatR;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Users.Commands;

/// <summary>
/// ユーザーを削除するコマンド。
/// Identity テーブルから物理削除する。過去の操作ログ（監査ログ）は別途テーブルで保持する想定。
/// </summary>
public record DeleteUserCommand(string Id) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserManagementService _userManagement;

    public DeleteUserCommandHandler(IUserManagementService userManagement)
    {
        _userManagement = userManagement;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var error = await _userManagement.DeleteUserAsync(request.Id, cancellationToken);

        if (error == "NotFound")
            throw new NotFoundException("User", request.Id);

        if (error is not null)
            throw new InvalidOperationException(error);
    }
}
