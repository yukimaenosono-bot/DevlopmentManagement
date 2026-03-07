using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Users.Commands;
using Synapse.Application.Users.Queries;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

/// <summary>
/// ユーザー管理 API。system-admin ロールのみアクセス可能。
/// 一般ユーザーが自分のプロフィールを変更する操作はここでは扱わない（別途実装予定）。
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "system-admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>ユーザー一覧を取得する。</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserListQuery(), ct);
        return Ok(result);
    }

    /// <summary>ユーザーを1件取得する。</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// ユーザーを新規作成する。
    /// ロール名は設計書（detailed-design/08）のロール定義を参照すること。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(
                new CreateUserCommand(request.UserName, request.DisplayName, request.Password, request.Roles), ct);

            // 201 Created + Location ヘッダーで新規リソースの URL を返す（REST の慣習）
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            // Identity のバリデーションエラー（パスワード強度・ユーザー名重複等）
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>ユーザーの表示名・ロールを更新する。</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateUserCommand(id, request.DisplayName, request.Roles), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// ユーザーを削除する（物理削除）。
    /// 削除前に製造実績・操作ログとの関係を確認すること。
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new DeleteUserCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>
/// ユーザー登録リクエスト。
/// roles には `detailed-design/08_バックエンド設計方針.md` で定義されたロール名を指定する。
/// </summary>
public record CreateUserRequest(
    string UserName,
    string DisplayName,
    string Password,
    IEnumerable<string> Roles
);

/// <summary>ユーザー更新リクエスト。UserName（ログイン名）の変更は含まない。</summary>
public record UpdateUserRequest(
    string DisplayName,
    IEnumerable<string> Roles
);
