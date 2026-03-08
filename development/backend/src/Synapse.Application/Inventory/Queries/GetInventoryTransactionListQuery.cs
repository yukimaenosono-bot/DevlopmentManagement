using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Inventory.Dtos;

namespace Synapse.Application.Inventory.Queries;

/// <summary>入出庫履歴を取得するクエリ。品目ID・倉庫ID・日時範囲でフィルタ可能。</summary>
public record GetInventoryTransactionListQuery(
    Guid? ItemId = null,
    Guid? WarehouseId = null,
    DateTime? From = null,
    DateTime? To = null) : IRequest<List<InventoryTransactionDto>>;

public class GetInventoryTransactionListQueryHandler
    : IRequestHandler<GetInventoryTransactionListQuery, List<InventoryTransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryTransactionListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryTransactionDto>> Handle(
        GetInventoryTransactionListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Item)
            .Include(t => t.Warehouse)
            .AsQueryable();

        if (request.ItemId.HasValue)
            query = query.Where(t => t.ItemId == request.ItemId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(t => t.WarehouseId == request.WarehouseId.Value);

        if (request.From.HasValue)
            query = query.Where(t => t.TransactedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(t => t.TransactedAt <= request.To.Value);

        return await query
            .OrderByDescending(t => t.TransactedAt)
            .Select(t => new InventoryTransactionDto(
                t.Id,
                t.ItemId, t.Item.Code, t.Item.Name,
                t.WarehouseId, t.Warehouse.Code, t.Warehouse.Name,
                t.LotNumber,
                t.TransactionType,
                t.Quantity,
                t.ReferenceNumber,
                t.Note,
                t.TransactedAt,
                t.CreatedByUserId))
            .ToListAsync(cancellationToken);
    }
}
