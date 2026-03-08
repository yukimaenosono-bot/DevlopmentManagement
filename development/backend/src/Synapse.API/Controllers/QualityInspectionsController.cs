using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.QualityInspections.Commands;
using Synapse.Application.QualityInspections.Dtos;
using Synapse.Application.QualityInspections.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/quality-inspections")]
[Authorize]
public class QualityInspectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QualityInspectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>品質検査一覧を取得する（QC-001〜003）。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QualityInspectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] InspectionType? inspectionType = null,
        [FromQuery] Guid? itemId = null,
        [FromQuery] InspectionResult? result = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken ct = default)
    {
        var results = await _mediator.Send(
            new GetQualityInspectionListQuery(inspectionType, itemId, result, from, to), ct);
        return Ok(results);
    }

    /// <summary>品質検査を1件取得する。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QualityInspectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetQualityInspectionByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>品質検査を登録する（QC-001〜003）。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateQualityInspectionRequest request, CancellationToken ct)
    {
        try
        {
            var inspectorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? string.Empty;

            var id = await _mediator.Send(new CreateQualityInspectionCommand(
                request.InspectionType,
                request.ItemId,
                request.LotNumber,
                request.WorkOrderId,
                request.InspectedAt ?? DateTime.UtcNow,
                inspectorUserId,
                request.InspectionQuantity,
                request.PassQuantity,
                request.FailQuantity,
                request.Result,
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
}

public record CreateQualityInspectionRequest(
    InspectionType InspectionType,
    Guid ItemId,
    string LotNumber,
    Guid? WorkOrderId,
    DateTime? InspectedAt,
    decimal InspectionQuantity,
    decimal PassQuantity,
    decimal FailQuantity,
    InspectionResult Result,
    string? Notes
);
