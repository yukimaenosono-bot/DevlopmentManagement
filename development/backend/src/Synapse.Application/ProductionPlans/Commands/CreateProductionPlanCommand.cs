using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ProductionPlans.Commands;

/// <summary>生産計画を新規作成する（PP-001）。戻り値は生成された計画の ID。</summary>
public record CreateProductionPlanCommand(
    Guid ItemId,
    decimal PlannedQuantity,
    DateOnly PlanStartDate,
    DateOnly PlanEndDate,
    DateOnly DueDate,
    PlanPriority Priority,
    string? Notes,
    string? OrderReference,
    string CreatedByUserId
) : IRequest<Guid>;

public class CreateProductionPlanCommandHandler : IRequestHandler<CreateProductionPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductionPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductionPlanCommand request, CancellationToken cancellationToken)
    {
        var itemExists = await _context.Items
            .AnyAsync(i => i.Id == request.ItemId && i.IsActive, cancellationToken);

        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        // 計画番号を採番する: PP-YYYYMM-NNNN
        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
        var prefix = $"PP-{yearMonth}-";
        var monthCount = await _context.ProductionPlans
            .CountAsync(p => p.PlanNumber.StartsWith(prefix), cancellationToken);
        var planNumber = $"{prefix}{monthCount + 1:0000}";

        var plan = ProductionPlan.Create(
            planNumber,
            request.ItemId,
            request.PlannedQuantity,
            request.PlanStartDate,
            request.PlanEndDate,
            request.DueDate,
            request.Priority,
            request.Notes,
            request.OrderReference,
            request.CreatedByUserId);

        _context.ProductionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
