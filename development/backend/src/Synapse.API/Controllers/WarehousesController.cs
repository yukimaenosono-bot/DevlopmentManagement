using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Warehouses.Commands;
using Synapse.Application.Warehouses.Dtos;
using Synapse.Application.Warehouses.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>倉庫マスタ一覧を取得する。activeOnly=false で廃止済みも含む。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWarehouseListQuery(activeOnly), ct);
        return Ok(result);
    }

    /// <summary>倉庫を新規作成する。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(
                new CreateWarehouseCommand(request.Code, request.Name, request.WarehouseType), ct);
            return CreatedAtAction(nameof(GetList), new { }, new { id });
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

    /// <summary>倉庫名・倉庫区分を更新する。</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateWarehouseCommand(id, request.Name, request.WarehouseType), ct);
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

    /// <summary>倉庫を廃止する（論理削除）。</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new DeactivateWarehouseCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>倉庫作成リクエスト。</summary>
public record CreateWarehouseRequest(string Code, string Name, WarehouseType WarehouseType);

/// <summary>倉庫更新リクエスト。</summary>
public record UpdateWarehouseRequest(string Name, WarehouseType WarehouseType);
