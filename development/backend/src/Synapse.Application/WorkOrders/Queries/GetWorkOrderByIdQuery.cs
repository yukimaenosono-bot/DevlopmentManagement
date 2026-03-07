using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.WorkOrders.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrders.Queries;

/// <summary>製造指示を1件取得する。存在しない場合は NotFoundException をスローする。</summary>
public record GetWorkOrderByIdQuery(Guid Id) : IRequest<WorkOrderDto>;

public class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkOrderDto> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var w = await _context.WorkOrders
            .Include(w => w.Item)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        return new WorkOrderDto(
            w.Id, w.WorkOrderNumber, w.ProductionPlanNumber,
            w.ItemId, w.Item.Code, w.Item.Name,
            w.LotNumber, w.Quantity,
            w.PlannedStartDate, w.PlannedEndDate,
            w.Status, w.WorkInstruction, w.CreatedByUserId,
            w.CreatedAt, w.UpdatedAt);
    }
}
