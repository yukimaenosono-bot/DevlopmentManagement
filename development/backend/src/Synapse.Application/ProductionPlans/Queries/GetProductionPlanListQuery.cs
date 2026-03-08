using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.ProductionPlans.Dtos;
using Synapse.Domain.Enums;

namespace Synapse.Application.ProductionPlans.Queries;

/// <summary>生産計画一覧を取得する（PP-002）。</summary>
public record GetProductionPlanListQuery(
    ProductionPlanStatus? Status = null,
    Guid? ItemId = null,
    DateOnly? From = null,
    DateOnly? To = null
) : IRequest<List<ProductionPlanDto>>;

public class GetProductionPlanListQueryHandler
    : IRequestHandler<GetProductionPlanListQuery, List<ProductionPlanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductionPlanListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductionPlanDto>> Handle(
        GetProductionPlanListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ProductionPlans
            .Include(p => p.Item)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.ItemId.HasValue)
            query = query.Where(p => p.ItemId == request.ItemId.Value);

        if (request.From.HasValue)
            query = query.Where(p => p.PlanStartDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(p => p.PlanStartDate <= request.To.Value);

        return await query
            .OrderByDescending(p => p.PlanNumber)
            .Select(p => new ProductionPlanDto(
                p.Id, p.PlanNumber,
                p.ItemId, p.Item.Code, p.Item.Name,
                p.PlannedQuantity, p.PlanStartDate, p.PlanEndDate, p.DueDate,
                p.Status, p.Priority, p.Notes, p.OrderReference,
                p.CreatedByUserId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
