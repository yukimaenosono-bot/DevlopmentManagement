using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Bom.Commands;
using Synapse.Application.Bom.Queries;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/bom")]
[Authorize]
public class BomController : ControllerBase
{
    private readonly IMediator _mediator;

    public BomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 指定した親品目の BOM（部品表）を取得する。
    /// asOf を指定すると有効期間内のラインのみ返す（製造指示発行時の引当計算で使用）。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBom(
        [FromQuery] Guid parentItemId,
        [FromQuery] DateOnly? asOf = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetBomQuery(parentItemId, asOf), ct);
        return Ok(result);
    }

    /// <summary>BOM に子品目ラインを追加する。</summary>
    [HttpPost]
    public async Task<IActionResult> AddLine([FromBody] AddBomLineRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new AddBomLineCommand(
                request.ParentItemId, request.ChildItemId,
                request.Quantity, request.Unit,
                request.ValidFrom, request.ValidTo), ct);

            // 204 No Content（リソース URL の概念がない複合キーのため 201 は使わない）
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // 重複ラインの登録試行
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // 循環参照・数量0・日付逆転等のドメインバリデーション違反
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>BOM ラインの数量・単位・有効期間を更新する。</summary>
    [HttpPut("{parentItemId:guid}/{childItemId:guid}")]
    public async Task<IActionResult> UpdateLine(
        Guid parentItemId, Guid childItemId,
        [FromBody] UpdateBomLineRequest request,
        CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateBomLineCommand(
                parentItemId, childItemId,
                request.Quantity, request.Unit,
                request.ValidFrom, request.ValidTo), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>BOM から子品目ラインを削除する。</summary>
    [HttpDelete("{parentItemId:guid}/{childItemId:guid}")]
    public async Task<IActionResult> RemoveLine(
        Guid parentItemId, Guid childItemId,
        CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new RemoveBomLineCommand(parentItemId, childItemId), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>BOM ライン追加リクエスト。</summary>
public record AddBomLineRequest(
    Guid ParentItemId,
    Guid ChildItemId,
    decimal Quantity,
    string Unit,
    DateOnly ValidFrom,
    DateOnly? ValidTo
);

/// <summary>BOM ライン更新リクエスト。親子品目の変更は不可（削除して再登録する運用）。</summary>
public record UpdateBomLineRequest(
    decimal Quantity,
    string Unit,
    DateOnly ValidFrom,
    DateOnly? ValidTo
);
