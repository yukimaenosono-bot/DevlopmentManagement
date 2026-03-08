using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Inventory.Commands;
using Synapse.Application.Inventory.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 在庫一覧を取得する。itemId・warehouseId でフィルタ可能。
    /// 在庫数量0の行も返す（在庫ゼロの管理目的）。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStock(
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? warehouseId = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetStockListQuery(itemId, warehouseId), ct);
        return Ok(result);
    }

    /// <summary>
    /// 入出庫履歴を取得する。itemId・warehouseId・日時範囲でフィルタ可能。
    /// 結果は処理日時の降順で返す。
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetInventoryTransactionListQuery(itemId, warehouseId, from, to), ct);
        return Ok(result);
    }

    /// <summary>
    /// 入庫を記録する。
    /// TransactionType は PurchaseReceipt/ManufacturingReceipt/ReturnReceipt/OtherReceipt のいずれかを指定する。
    /// </summary>
    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] ReceiveInventoryRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            await _mediator.Send(new ReceiveInventoryCommand(
                request.ItemId, request.WarehouseId, request.LotNumber,
                request.TransactionType, request.Quantity,
                request.ReferenceNumber, request.Note, userId), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 出庫を記録する。在庫不足の場合は 400 を返す。
    /// </summary>
    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IssueInventoryRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            await _mediator.Send(new IssueInventoryCommand(
                request.ItemId, request.WarehouseId, request.LotNumber,
                request.Quantity, request.ReferenceNumber, request.Note, userId), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 棚卸調整を記録する。NewQuantity に実地棚卸の実測値を指定する。
    /// 帳簿在庫との差分を自動計算して履歴に記録する。
    /// </summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustInventoryRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;
            await _mediator.Send(new AdjustInventoryCommand(
                request.ItemId, request.WarehouseId, request.LotNumber,
                request.NewQuantity, request.ReferenceNumber, request.Note, userId), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>入庫リクエスト。</summary>
public record ReceiveInventoryRequest(
    Guid ItemId,
    Guid WarehouseId,
    string? LotNumber,
    InventoryTransactionType TransactionType,
    decimal Quantity,
    string? ReferenceNumber,
    string? Note);

/// <summary>出庫リクエスト。</summary>
public record IssueInventoryRequest(
    Guid ItemId,
    Guid WarehouseId,
    string? LotNumber,
    decimal Quantity,
    string? ReferenceNumber,
    string? Note);

/// <summary>棚卸調整リクエスト。NewQuantity に実地棚卸の実測値を指定する。</summary>
public record AdjustInventoryRequest(
    Guid ItemId,
    Guid WarehouseId,
    string? LotNumber,
    decimal NewQuantity,
    string? ReferenceNumber,
    string? Note);
