using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.ProductionPlans.Commands;
using Synapse.Application.ProductionPlans.Dtos;
using Synapse.Application.ProductionPlans.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/production-plans")]
[Authorize]
public class ProductionPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductionPlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>生産計画一覧を取得する（PP-002）。ステータス・品目・日付範囲で絞り込み可能。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] ProductionPlanStatus? status = null,
        [FromQuery] Guid? itemId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProductionPlanListQuery(status, itemId, from, to), ct);
        return Ok(result);
    }

    /// <summary>生産計画を1件取得する（PP-002）。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetProductionPlanByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>生産計画を新規作成する（PP-001）。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateProductionPlanRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var id = await _mediator.Send(new CreateProductionPlanCommand(
                request.ItemId,
                request.PlannedQuantity,
                request.PlanStartDate,
                request.PlanEndDate,
                request.DueDate,
                request.Priority,
                request.Notes,
                request.OrderReference,
                userId), ct);

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

    /// <summary>生産計画を更新する。仮計画のみ変更可能。</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductionPlanRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateProductionPlanCommand(
                id,
                request.PlannedQuantity,
                request.PlanStartDate,
                request.PlanEndDate,
                request.DueDate,
                request.Priority,
                request.Notes,
                request.OrderReference), ct);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>生産計画を確定する（PP-001）。確定後は製造指示への展開が可能になる。</summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new ConfirmProductionPlanCommand(id), ct);
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

    /// <summary>生産計画をキャンセルする。</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new CancelProductionPlanCommand(id), ct);
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

    /// <summary>確定済み生産計画から製造指示を展開する（PP-004）。</summary>
    [HttpPost("{id:guid}/expand")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Expand(Guid id, [FromBody] ExpandProductionPlanRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var workOrderId = await _mediator.Send(new ExpandToWorkOrdersCommand(
                id,
                request.LotNumber,
                request.WorkInstruction,
                userId), ct);

            return CreatedAtAction(
                "GetById",
                "WorkOrders",
                new { id = workOrderId },
                new { workOrderId });
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

public record CreateProductionPlanRequest(
    Guid ItemId,
    decimal PlannedQuantity,
    DateOnly PlanStartDate,
    DateOnly PlanEndDate,
    DateOnly DueDate,
    PlanPriority Priority,
    string? Notes,
    string? OrderReference
);

public record UpdateProductionPlanRequest(
    decimal PlannedQuantity,
    DateOnly PlanStartDate,
    DateOnly PlanEndDate,
    DateOnly DueDate,
    PlanPriority Priority,
    string? Notes,
    string? OrderReference
);

public record ExpandProductionPlanRequest(
    string? LotNumber,
    string? WorkInstruction
);
