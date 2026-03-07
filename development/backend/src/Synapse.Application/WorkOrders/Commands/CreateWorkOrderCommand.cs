using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrders.Commands;

/// <summary>製造指示を発行するコマンド。戻り値は生成された製造指示の ID。</summary>
public record CreateWorkOrderCommand(
    string? ProductionPlanNumber,
    Guid ItemId,
    string? LotNumber,
    decimal Quantity,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    string? WorkInstruction,
    string CreatedByUserId
) : IRequest<Guid>;

public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        // 対象品目が存在するか確認する
        var itemExists = await _context.Items
            .AnyAsync(i => i.Id == request.ItemId && i.IsActive, cancellationToken);

        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        // 製造指示番号を採番する: MO-YYYYMMDD-NNNN
        // 同日の既存件数を取得して連番とする。DB のユニーク制約が最終的な重複防止になる。
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"MO-{today}-";
        var todayCount = await _context.WorkOrders
            .CountAsync(w => w.WorkOrderNumber.StartsWith(prefix), cancellationToken);
        var workOrderNumber = $"{prefix}{todayCount + 1:0000}";

        var workOrder = WorkOrder.Create(
            workOrderNumber,
            request.ProductionPlanNumber,
            request.ItemId,
            request.LotNumber,
            request.Quantity,
            request.PlannedStartDate,
            request.PlannedEndDate,
            request.WorkInstruction,
            request.CreatedByUserId);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return workOrder.Id;
    }
}
