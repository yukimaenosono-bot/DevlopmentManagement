using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ShipmentOrders.Commands;

/// <summary>出荷実績を登録し出荷済みに移行する（SH-004）。在庫から出荷数量を減算する。</summary>
public record ShipCommand(
    Guid Id,
    decimal ActualQuantity,
    string? LotNumber,
    DateTime ShippedAt,
    string ShippedByUserId,
    Guid WarehouseId
) : IRequest;

public class ShipCommandHandler : IRequestHandler<ShipCommand>
{
    private readonly IApplicationDbContext _context;

    public ShipCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ShipCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.ShipmentOrders
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ShipmentOrder), request.Id);

        var lotNumber = request.LotNumber ?? order.LotNumber;

        // 在庫から出荷数量を減算する
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s =>
                s.ItemId == order.ItemId &&
                s.WarehouseId == request.WarehouseId &&
                (lotNumber == null || s.LotNumber == lotNumber),
            cancellationToken)
            ?? throw new InvalidOperationException("在庫が見つかりません。品目・倉庫・ロットを確認してください。");

        if (stock.Quantity < request.ActualQuantity)
            throw new InvalidOperationException(
                $"在庫数量が不足しています。在庫: {stock.Quantity}、出荷要求: {request.ActualQuantity}");

        stock.Issue(request.ActualQuantity);

        // 出荷実績を記録する
        var transaction = InventoryTransaction.Create(
            order.ItemId,
            request.WarehouseId,
            lotNumber,
            InventoryTransactionType.Issue,
            request.ActualQuantity,
            order.ShipmentNumber,
            null,
            request.ShippedByUserId);
        _context.InventoryTransactions.Add(transaction);

        order.Ship(request.ActualQuantity, lotNumber, request.ShippedAt, request.ShippedByUserId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
