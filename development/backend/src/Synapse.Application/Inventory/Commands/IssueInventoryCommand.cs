using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;

namespace Synapse.Application.Inventory.Commands;

/// <summary>
/// 出庫コマンド。原材料の製造払出・完成品の出荷などに対応する。
/// 在庫行が存在しない場合、または在庫不足の場合はエラーとする。
/// </summary>
public record IssueInventoryCommand(
    Guid ItemId,
    Guid WarehouseId,
    string? LotNumber,
    decimal Quantity,
    string? ReferenceNumber,
    string? Note,
    string CreatedByUserId) : IRequest;

public class IssueInventoryCommandHandler : IRequestHandler<IssueInventoryCommand>
{
    private readonly IApplicationDbContext _context;

    public IssueInventoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(IssueInventoryCommand request, CancellationToken cancellationToken)
    {
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s =>
                s.ItemId == request.ItemId &&
                s.WarehouseId == request.WarehouseId &&
                s.LotNumber == request.LotNumber, cancellationToken)
            ?? throw new InvalidOperationException(
                "対象の在庫行が見つかりません。在庫のない品目は出庫できません。");

        stock.Issue(request.Quantity);

        var transaction = InventoryTransaction.Create(
            request.ItemId, request.WarehouseId, request.LotNumber,
            InventoryTransactionType.Issue, request.Quantity,
            request.ReferenceNumber, request.Note, request.CreatedByUserId);
        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
