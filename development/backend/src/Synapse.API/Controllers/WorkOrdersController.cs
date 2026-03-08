using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.WorkOrders.Commands;
using Synapse.Application.WorkOrders.Dtos;
using Synapse.Application.WorkOrders.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/work-orders")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 製造指示一覧を取得する。
    /// ステータス・日付範囲でフィルタリング可能（製造指示一覧画面 SCR-MO-001 対応）。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] WorkOrderStatus? status = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWorkOrderListQuery(status, from, to), ct);
        return Ok(result);
    }

    /// <summary>製造指示を1件取得する。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetWorkOrderByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 製造指示を発行する（MO-001）。
    /// 製造指示番号は MO-YYYYMMDD-NNNN 形式で自動採番される。
    /// 発行者は JWT クレームから自動取得する。
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken ct)
    {
        try
        {
            // 発行者のユーザー ID を JWT クレームから取得する
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var id = await _mediator.Send(new CreateWorkOrderCommand(
                request.ProductionPlanNumber,
                request.ItemId,
                request.LotNumber,
                request.Quantity,
                request.PlannedStartDate,
                request.PlannedEndDate,
                request.WorkInstruction,
                userId), ct);

            // 201 Created + Location ヘッダーで新規リソースの URL を返す（REST の慣習）
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (NotFoundException ex)
        {
            // 存在しない品目 ID が指定された場合
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // 数量・日付のドメインバリデーション違反
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 製造指示を更新する（MO-003）。
    /// 完了・キャンセル済みは更新不可。着手中は数量変更不可。
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkOrderRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateWorkOrderCommand(
                id, request.Quantity,
                request.PlannedStartDate, request.PlannedEndDate,
                request.WorkInstruction), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // ステータス制約違反（完了済み変更不可・着手中数量変更不可）
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>製造指示をキャンセルする（MO-004）。発行済・着手中のみ可能。</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new CancelWorkOrderCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // 完了済みへのキャンセル試行等
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 製造指示を着手中に移行する（MO-007）。
    /// 発行済（Issued）からのみ遷移可能。着手後は指示数量が変更不可になる。
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new StartWorkOrderCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // 発行済以外からの着手試行
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 製造指示を完了に移行する（MO-007）。
    /// 着手中（InProgress）からのみ遷移可能。完了後は変更・キャンセル不可。
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new CompleteWorkOrderCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // 着手中以外からの完了試行
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// 製造指示発行リクエスト。
/// ロット管理品目（Item.IsLotManaged=true）では LotNumber の指定を推奨する。
/// </summary>
public record CreateWorkOrderRequest(
    string? ProductionPlanNumber,
    Guid ItemId,
    string? LotNumber,
    decimal Quantity,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    string? WorkInstruction
);

/// <summary>
/// 製造指示更新リクエスト。
/// 着手中は Quantity の変更が拒否される（ドメインルール）。
/// </summary>
public record UpdateWorkOrderRequest(
    decimal Quantity,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    string? WorkInstruction
);
