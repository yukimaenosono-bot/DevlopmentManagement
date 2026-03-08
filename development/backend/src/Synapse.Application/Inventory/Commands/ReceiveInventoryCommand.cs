using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Inventory.Commands;

/// <summary>
/// 入庫コマンド。PurchaseReceipt/ManufacturingReceipt/ReturnReceipt/OtherReceipt に対応する。
/// 在庫行が存在しない場合は新規作成する（初回入庫で在庫行が生まれる）。
/// </summary>
public record ReceiveInventoryCommand(
    Guid ItemId,
    Guid WarehouseId,
    string? LotNumber,
    InventoryTransactionType TransactionType,
    decimal Quantity,
    string? ReferenceNumber,
    string? Note,
    string CreatedByUserId) : IRequest;

public class ReceiveInventoryCommandHandler : IRequestHandler<ReceiveInventoryCommand>
{
    private readonly IApplicationDbContext _context;

    public ReceiveInventoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReceiveInventoryCommand request, CancellationToken cancellationToken)
    {
        // 入庫系以外の TransactionType は受け付けない
        if (request.TransactionType is InventoryTransactionType.Issue or InventoryTransactionType.Adjustment)
            throw new ArgumentException(
                "入庫コマンドには PurchaseReceipt/ManufacturingReceipt/ReturnReceipt/OtherReceipt を指定してください。");

        var itemExists = await _context.Items.AnyAsync(i => i.Id == request.ItemId, cancellationToken);
        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);
        if (!warehouseExists)
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

        // 同一品目・倉庫・ロットの在庫行を取得（なければ新規作成）
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s =>
                s.ItemId == request.ItemId &&
                s.WarehouseId == request.WarehouseId &&
                s.LotNumber == request.LotNumber, cancellationToken);

        if (stock is null)
        {
            stock = Stock.Create(request.ItemId, request.WarehouseId, request.LotNumber);
            _context.Stocks.Add(stock);
        }

        stock.Receive(request.Quantity);

        var transaction = InventoryTransaction.Create(
            request.ItemId, request.WarehouseId, request.LotNumber,
            request.TransactionType, request.Quantity,
            request.ReferenceNumber, request.Note, request.CreatedByUserId);
        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
