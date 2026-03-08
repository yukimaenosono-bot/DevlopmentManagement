using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Routings.Commands;
using Synapse.Application.Routings.Dtos;
using Synapse.Application.Routings.Queries;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/routings")]
[Authorize]
public class RoutingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoutingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>ルーティング一覧を取得する。itemId で品目絞り込み可能。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoutingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? itemId = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetRoutingListQuery(itemId, activeOnly), ct);
        return Ok(result);
    }

    /// <summary>ルーティングを1件取得する（ステップ含む）。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoutingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetRoutingByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>ルーティングとステップを新規作成する。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoutingRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(new CreateRoutingCommand(
                request.ItemId, request.Name, request.IsDefault,
                request.Steps.Select(s => new CreateRoutingStepRequest(
                    s.Sequence, s.ProcessId, s.EquipmentId, s.StandardTime, s.IsRequired))), ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
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

    /// <summary>ルーティングを更新する（ステップは全件置き換え）。</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoutingRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateRoutingCommand(
                id, request.Name, request.IsDefault,
                request.Steps.Select(s => new UpdateRoutingStepRequest(
                    s.Sequence, s.ProcessId, s.EquipmentId, s.StandardTime, s.IsRequired))), ct);
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

    /// <summary>ルーティングを非アクティブ化する（論理削除）。</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new DeleteRoutingCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public record CreateRoutingRequest(
    Guid ItemId,
    string Name,
    bool IsDefault,
    IEnumerable<RoutingStepRequest> Steps
);

public record UpdateRoutingRequest(
    string Name,
    bool IsDefault,
    IEnumerable<RoutingStepRequest> Steps
);

public record RoutingStepRequest(
    int Sequence,
    Guid ProcessId,
    Guid? EquipmentId,
    decimal? StandardTime,
    bool IsRequired
);
