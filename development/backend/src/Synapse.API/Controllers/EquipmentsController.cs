using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Equipments.Commands;
using Synapse.Application.Equipments.Queries;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EquipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 設備一覧を取得する。
    /// processId を指定すると特定工程の設備のみ返す（工程実績入力画面での絞り込み用途）。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] Guid? processId = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetEquipmentListQuery(processId, activeOnly), ct);
        return Ok(result);
    }

    /// <summary>設備を1件取得する。</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetEquipmentByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>設備を新規作成する。設備コード重複時・存在しない工程指定時は 400 を返す。</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(
                new CreateEquipmentCommand(request.Code, request.Name, request.ProcessId), ct);

            // 201 Created + Location ヘッダーで新規リソースの URL を返す（REST の慣習）
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            // 設備コード重複などの業務ルール違反
            return BadRequest(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            // 存在しない工程 ID が指定された場合
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>設備を更新する。</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEquipmentRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(
                new UpdateEquipmentCommand(id, request.Name, request.ProcessId, request.IsActive), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 設備を廃棄・撤去扱いにする（論理削除）。
    /// 物理削除は行わない。過去の工程実績が設備コードを参照しているため。
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new DeleteEquipmentCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>設備登録リクエスト。Code は登録後に変更できないため、慎重に設定すること。</summary>
public record CreateEquipmentRequest(
    string Code,
    string Name,
    Guid ProcessId
);

/// <summary>
/// 設備更新リクエスト。Code（設備コード）は変更不可のためフィールドに含まない。
/// IsActive=false にすると廃棄・撤去扱いになる（DELETE エンドポイントと同じ効果）。
/// </summary>
public record UpdateEquipmentRequest(
    string Name,
    Guid ProcessId,
    bool IsActive
);
