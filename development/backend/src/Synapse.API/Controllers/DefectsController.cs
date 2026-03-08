using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Defects.Commands;
using Synapse.Application.Defects.Dtos;
using Synapse.Application.Defects.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/defects")]
[Authorize]
public class DefectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DefectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>不良一覧を取得する（QC-004）。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DefectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? itemId = null,
        [FromQuery] Guid? workOrderId = null,
        [FromQuery] DefectCategory? category = null,
        [FromQuery] DispositionType? disposition = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var results = await _mediator.Send(
            new GetDefectListQuery(itemId, workOrderId, category, disposition, from, to), ct);
        return Ok(results);
    }

    /// <summary>不良を1件取得する。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DefectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetDefectByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>不良を登録する（QC-004）。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateDefectRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(new CreateDefectCommand(
                request.QualityInspectionId,
                request.WorkOrderId,
                request.ItemId,
                request.OccurredAt ?? DateTime.UtcNow,
                request.ProcessId,
                request.Category,
                request.Description,
                request.Quantity,
                request.EstimatedCause,
                request.CorrectiveAction,
                request.Disposition,
                request.DispositionNote), ct);

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

    /// <summary>不良品処置を更新する（QC-005）。</summary>
    [HttpPatch("{id:guid}/disposition")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDisposition(
        Guid id, [FromBody] UpdateDispositionRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateDefectDispositionCommand(
                id,
                request.Disposition,
                request.DispositionNote,
                request.EstimatedCause,
                request.CorrectiveAction), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public record CreateDefectRequest(
    Guid? QualityInspectionId,
    Guid? WorkOrderId,
    Guid ItemId,
    DateTime? OccurredAt,
    Guid? ProcessId,
    DefectCategory Category,
    string Description,
    decimal Quantity,
    string? EstimatedCause,
    string? CorrectiveAction,
    DispositionType Disposition,
    string? DispositionNote
);

public record UpdateDispositionRequest(
    DispositionType Disposition,
    string? DispositionNote,
    string? EstimatedCause,
    string? CorrectiveAction
);
