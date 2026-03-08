using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Inventory.Commands;

/// <summary>
/// 棚卸調整コマンド。帳簿在庫と実地在庫の差異を調整する。
/// 在庫行が存在しない場合は新規作成する（実地棚卸で初めて確認された在庫）。
/// 履歴には差分値（正負）を記録する。
/// </summary>
public record AdjustInventoryCommand(
    Guid ItemId,
    Guid WarehouseId,
    string? LotNumber,
    decimal NewQuantity,
    string? ReferenceNumber,
    string? Note,
    string CreatedByUserId) : IRequest;

public class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand>
{
    private readonly IApplicationDbContext _context;

    public AdjustInventoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
    {
        var itemExists = await _context.Items.AnyAsync(i => i.Id == request.ItemId, cancellationToken);
        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (!warehouseExists)
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s =>
                s.ItemId == request.ItemId &&
                s.WarehouseId == request.WarehouseId &&
                s.LotNumber == request.LotNumber, cancellationToken);

        // 差分値を計算して履歴に記録（正=増加、負=減少）
        var delta = stock is not null
            ? request.NewQuantity - stock.Quantity
            : request.NewQuantity;

        if (stock is null)
        {
            stock = Stock.Create(request.ItemId, request.WarehouseId, request.LotNumber);
            _context.Stocks.Add(stock);
        }

        stock.Adjust(request.NewQuantity);

        var transaction = InventoryTransaction.Create(
            request.ItemId, request.WarehouseId, request.LotNumber,
            InventoryTransactionType.Adjustment, delta,
            request.ReferenceNumber, request.Note, request.CreatedByUserId);
        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
