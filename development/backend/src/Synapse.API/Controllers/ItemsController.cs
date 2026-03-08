using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Items.Commands;
using Synapse.Application.Items.Dtos;
using Synapse.Application.Items.Queries;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>品目一覧を取得する。</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetItemListQuery(activeOnly), ct);
        return Ok(result);
    }

    /// <summary>品目を1件取得する。</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetItemByIdQuery(id), ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>品目を新規作成する。品目コード重複時は 400 を返す。</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest request, CancellationToken ct)
    {
        try
        {
            var id = await _mediator.Send(new CreateItemCommand(
                request.Code, request.Name, request.ShortName, request.ItemType,
                request.Unit, request.StandardUnitPrice, request.SafetyStockQuantity,
                request.HasExpirationDate, request.IsLotManaged), ct);

            // 201 Created + Location ヘッダーで新規リソースの URL を返す（REST の慣習）
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            // 品目コード重複などの業務ルール違反
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>品目を更新する。</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new UpdateItemCommand(
                id, request.Name, request.ShortName, request.ItemType,
                request.Unit, request.StandardUnitPrice, request.SafetyStockQuantity,
                request.HasExpirationDate, request.IsLotManaged, request.IsActive), ct);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 品目を廃番にする（論理削除）。
    /// 物理削除は行わない。過去の製造実績・在庫履歴が品目名を参照しているため。
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(new DeleteItemCommand(id), ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

/// <summary>品目登録リクエスト。Code は登録後に変更できないため、慎重に設定すること。</summary>
public record CreateItemRequest(
    string Code,
    string Name,
    string? ShortName,
    ItemType ItemType,
    string Unit,
    decimal StandardUnitPrice,
    decimal SafetyStockQuantity,
    bool HasExpirationDate,
    bool IsLotManaged
);

/// <summary>
/// 品目更新リクエスト。Code（品目コード）は変更不可のためフィールドに含まない。
/// IsActive=false にすると廃番扱いになる（DELETE エンドポイントと同じ効果）。
/// </summary>
public record UpdateItemRequest(
    string Name,
    string? ShortName,
    ItemType ItemType,
    string Unit,
    decimal StandardUnitPrice,
    decimal SafetyStockQuantity,
    bool HasExpirationDate,
    bool IsLotManaged,
    bool IsActive
);
