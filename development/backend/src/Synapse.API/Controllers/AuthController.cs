using MediatR;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Auth.Commands;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>ログイン。成功するとJWTトークンを返す。</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new LoginCommand(request.UserName, request.Password), ct);
            return Ok(new { token = result.Token });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "ユーザー名またはパスワードが正しくありません。" });
        }
    }
}

public record LoginRequest(string UserName, string Password);
