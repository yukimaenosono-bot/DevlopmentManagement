using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Items.Dtos;

namespace Synapse.Application.Items.Queries;

/// <summary>品目一覧を取得する。IsActive=true のみを返すのがデフォルト。</summary>
public record GetItemListQuery(bool ActiveOnly = true) : IRequest<List<ItemDto>>;

public class GetItemListQueryHandler : IRequestHandler<GetItemListQuery, List<ItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetItemListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemDto>> Handle(GetItemListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Items.AsQueryable();

        // 製造指示・発注の品目選択画面では廃番品を表示しないのがデフォルト。
        // 廃番品も含めて参照したい場合（マスタ管理画面等）は activeOnly=false で呼ぶ。
        if (request.ActiveOnly)
            query = query.Where(i => i.IsActive);

        // 品目コード順で表示するのが業務上の慣習
        return await query
            .OrderBy(i => i.Code)
            .Select(i => new ItemDto(
                i.Id, i.Code, i.Name, i.ShortName, i.ItemType,
                i.Unit, i.StandardUnitPrice, i.SafetyStockQuantity,
                i.HasExpirationDate, i.IsLotManaged, i.IsActive,
                i.CreatedAt, i.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
