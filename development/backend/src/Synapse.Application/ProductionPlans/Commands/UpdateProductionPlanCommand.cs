using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ProductionPlans.Commands;

/// <summary>生産計画の内容を更新する。仮計画（Draft）のみ変更可能。</summary>
public record UpdateProductionPlanCommand(
    Guid Id,
    decimal PlannedQuantity,
    DateOnly PlanStartDate,
    DateOnly PlanEndDate,
    DateOnly DueDate,
    PlanPriority Priority,
    string? Notes,
    string? OrderReference
) : IRequest;

public class UpdateProductionPlanCommandHandler : IRequestHandler<UpdateProductionPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductionPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProductionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.ProductionPlans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductionPlan), request.Id);

        plan.Update(
            request.PlannedQuantity,
            request.PlanStartDate,
            request.PlanEndDate,
            request.DueDate,
            request.Priority,
            request.Notes,
            request.OrderReference);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
