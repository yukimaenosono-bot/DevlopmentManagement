using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Processes.Commands;
using Synapse.Application.Processes.Dtos;
using Synapse.Application.Processes.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcessesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProcessesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>工程一覧を取得する。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProcessDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProcessListQuery(activeOnly), ct);
        return Ok(result);
    }

    /// <summary>工程を1件取得する。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProcessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetProcessByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>工程を新規作成する。工程コード重複時は 400 を返す。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProcessRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(
                new CreateProcessCommand(request.Code, request.Name, request.ProcessType), ct);

            // 201 Created + Location ヘッダーで新規リソースの URL を返す（REST の慣習）
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            // 工程コード重複などの業務ルール違反
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>工程を更新する。</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProcessRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(
                new UpdateProcessCommand(id, request.Name, request.ProcessType, request.IsActive), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 工程を廃止にする（論理削除）。
    /// 物理削除は行わない。過去の工程実績・ルーティングが工程コードを参照しているため。
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new DeleteProcessCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>工程登録リクエスト。Code は登録後に変更できないため、慎重に設定すること。</summary>
public record CreateProcessRequest(
    string Code,
    string Name,
    ProcessType ProcessType
);

/// <summary>
/// 工程更新リクエスト。Code（工程コード）は変更不可のためフィールドに含まない。
/// IsActive=false にすると廃止扱いになる（DELETE エンドポイントと同じ効果）。
/// </summary>
public record UpdateProcessRequest(
    string Name,
    ProcessType ProcessType,
    bool IsActive
);
