using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.ProductionPlans.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ProductionPlans.Queries;

/// <summary>生産計画を1件取得する。</summary>
public record GetProductionPlanByIdQuery(Guid Id) : IRequest<ProductionPlanDto>;

public class GetProductionPlanByIdQueryHandler
    : IRequestHandler<GetProductionPlanByIdQuery, ProductionPlanDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductionPlanByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductionPlanDto> Handle(
        GetProductionPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _context.ProductionPlans
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductionPlan), request.Id);

        return new ProductionPlanDto(
            plan.Id, plan.PlanNumber,
            plan.ItemId, plan.Item.Code, plan.Item.Name,
            plan.PlannedQuantity, plan.PlanStartDate, plan.PlanEndDate, plan.DueDate,
            plan.Status, plan.Priority, plan.Notes, plan.OrderReference,
            plan.CreatedByUserId, plan.CreatedAt, plan.UpdatedAt);
    }
}
