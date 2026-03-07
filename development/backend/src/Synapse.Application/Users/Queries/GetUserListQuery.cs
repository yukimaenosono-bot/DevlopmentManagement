using MediatR;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Users.Dtos;

namespace Synapse.Application.Users.Queries;

/// <summary>ユーザー一覧を取得する。システム管理者（system-admin）のみ使用可。</summary>
public record GetUserListQuery : IRequest<List<UserDto>>;

public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, List<UserDto>>
{
    private readonly IUserManagementService _userManagement;

    public GetUserListQueryHandler(IUserManagementService userManagement)
    {
        _userManagement = userManagement;
    }

    public Task<List<UserDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
        => _userManagement.GetUsersAsync(cancellationToken);
}
