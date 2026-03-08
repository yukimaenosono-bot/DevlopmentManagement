using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ProductionPlans.Commands;

/// <summary>確定済み生産計画から製造指示を展開する（PP-004）。戻り値は生成された製造指示の ID。</summary>
public record ExpandToWorkOrdersCommand(
    Guid ProductionPlanId,
    string? LotNumber,
    string? WorkInstruction,
    string CreatedByUserId
) : IRequest<Guid>;

public class ExpandToWorkOrdersCommandHandler : IRequestHandler<ExpandToWorkOrdersCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ExpandToWorkOrdersCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ExpandToWorkOrdersCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.ProductionPlans
            .FirstOrDefaultAsync(p => p.Id == request.ProductionPlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductionPlan), request.ProductionPlanId);

        // MarkAsInProgress() 内でステータス検証を行う（Confirmed でなければ例外）
        plan.MarkAsInProgress();

        // 製造指示番号を採番する: MO-YYYYMMDD-NNNN
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"MO-{today}-";
        var todayCount = await _context.WorkOrders
            .CountAsync(w => w.WorkOrderNumber.StartsWith(prefix), cancellationToken);
        var workOrderNumber = $"{prefix}{todayCount + 1:0000}";

        var workOrder = WorkOrder.Create(
            workOrderNumber,
            plan.PlanNumber,
            plan.ItemId,
            request.LotNumber,
            plan.PlannedQuantity,
            plan.PlanStartDate,
            plan.PlanEndDate,
            request.WorkInstruction,
            request.CreatedByUserId);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return workOrder.Id;
    }
}
