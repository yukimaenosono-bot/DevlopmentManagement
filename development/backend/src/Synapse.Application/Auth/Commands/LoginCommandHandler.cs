using MediatR;
using Synapse.Application.Common.Interfaces;

namespace Synapse.Application.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (success, token) = await _identityService.LoginAsync(request.UserName, request.Password);

        if (!success)
            throw new UnauthorizedAccessException("ユーザー名またはパスワードが正しくありません。");

        return new LoginResult(token);
    }
}
