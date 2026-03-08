using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.ShipmentOrders.Commands;
using Synapse.Application.ShipmentOrders.Dtos;
using Synapse.Application.ShipmentOrders.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/shipment-orders")]
[Authorize]
public class ShipmentOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>出荷指示一覧を取得する（SH-002）。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShipmentOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] ShipmentOrderStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetShipmentOrderListQuery(status, customerId, from, to), ct);
        return Ok(result);
    }

    /// <summary>出荷指示を1件取得する。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShipmentOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetShipmentOrderByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>出荷指示を新規作成する（SH-001）。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentOrderRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(new CreateShipmentOrderCommand(
                request.OrderReference,
                request.CustomerId,
                request.ItemId,
                request.PlannedQuantity,
                request.PlannedShipDate,
                request.LotNumber,
                request.Carrier,
                request.Notes), ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
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

    /// <summary>出荷指示を確定する（出荷待ち）。</summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new ConfirmShipmentOrderCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>ピッキング完了に移行する（SH-003）。</summary>
    [HttpPost("{id:guid}/pick")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pick(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new MarkShipmentPickedCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>出荷実績を登録する（SH-004）。在庫から出荷数量を減算する。</summary>
    [HttpPost("{id:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ship(Guid id, [FromBody] ShipRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            await _mediator.Send(new ShipCommand(
                id,
                request.ActualQuantity,
                request.LotNumber,
                request.ShippedAt ?? DateTime.UtcNow,
                userId,
                request.WarehouseId), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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

    /// <summary>出荷指示をキャンセルする。</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new CancelShipmentOrderCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record CreateShipmentOrderRequest(
    string OrderReference,
    Guid CustomerId,
    Guid ItemId,
    decimal PlannedQuantity,
    DateOnly PlannedShipDate,
    string? LotNumber,
    string? Carrier,
    string? Notes
);

public record ShipRequest(
    decimal ActualQuantity,
    string? LotNumber,
    DateTime? ShippedAt,
    Guid WarehouseId
);
