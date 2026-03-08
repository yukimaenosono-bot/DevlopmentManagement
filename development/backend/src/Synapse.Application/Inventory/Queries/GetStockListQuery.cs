using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Inventory.Dtos;

namespace Synapse.Application.Inventory.Queries;

/// <summary>在庫一覧を取得するクエリ。品目ID・倉庫IDでフィルタ可能。</summary>
public record GetStockListQuery(Guid? ItemId = null, Guid? WarehouseId = null) : IRequest<List<StockDto>>;

public class GetStockListQueryHandler : IRequestHandler<GetStockListQuery, List<StockDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockDto>> Handle(GetStockListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Stocks
            .Include(s => s.Item)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (request.ItemId.HasValue)
            query = query.Where(s => s.ItemId == request.ItemId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);

        return await query
            .OrderBy(s => s.Item.Code)
            .ThenBy(s => s.Warehouse.Code)
            .Select(s => new StockDto(
                s.Id,
                s.ItemId, s.Item.Code, s.Item.Name, s.Item.Unit,
                s.WarehouseId, s.Warehouse.Code, s.Warehouse.Name,
                s.LotNumber,
                s.Quantity,
                s.LastTransactedAt))
            .ToListAsync(cancellationToken);
    }
}
