using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.WorkOrderOperations.Commands;
using Synapse.Application.WorkOrderOperations.Dtos;
using Synapse.Application.WorkOrderOperations.Queries;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Authorize]
public class OperationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>製造指示に紐付く工程実績一覧を取得する（WO-003）。</summary>
    [HttpGet("api/work-orders/{workOrderId:guid}/operations")]
    [ProducesResponseType(typeof(IEnumerable<WorkOrderOperationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOperations(Guid workOrderId, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetWorkOrderOperationsQuery(workOrderId), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// ルーティングから工程実績を生成する。
    /// routingId を省略するとデフォルトルーティングを使用する。
    /// </summary>
    [HttpPost("api/work-orders/{workOrderId:guid}/operations/generate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateOperations(
        Guid workOrderId,
        [FromBody] GenerateOperationsRequest request,
        CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new GenerateOperationsCommand(workOrderId, request.RoutingId), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>工程を着手中に移行する（WO-001）。</summary>
    [HttpPost("api/operations/{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;
            await _mediator.Send(new StartOperationCommand(id, userId, DateTime.UtcNow), ct);
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

    /// <summary>工程を完了に移行し実績を記録する（WO-002）。</summary>
    [HttpPost("api/operations/{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteOperationRequest request,
        CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new CompleteOperationCommand(
                id, request.ActualQuantity, request.DefectQuantity, DateTime.UtcNow, request.Notes), ct);
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

    /// <summary>工程を保留にする。</summary>
    [HttpPost("api/operations/{id:guid}/hold")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Hold(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new PutOperationOnHoldCommand(id), ct);
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

public record GenerateOperationsRequest(Guid? RoutingId);

public record CompleteOperationRequest(
    decimal ActualQuantity,
    decimal DefectQuantity,
    string? Notes
);
