using MediatR;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Users.Dtos;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Users.Queries;

/// <summary>ユーザーを1件取得する。存在しない場合は NotFoundException をスローする。</summary>
public record GetUserByIdQuery(string Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserManagementService _userManagement;

    public GetUserByIdQueryHandler(IUserManagementService userManagement)
    {
        _userManagement = userManagement;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManagement.GetUserByIdAsync(request.Id, cancellationToken);
        return user ?? throw new NotFoundException("User", request.Id);
    }
}
