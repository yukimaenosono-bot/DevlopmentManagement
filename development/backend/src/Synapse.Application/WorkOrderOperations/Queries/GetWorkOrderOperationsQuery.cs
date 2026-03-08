using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.WorkOrderOperations.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrderOperations.Queries;

/// <summary>製造指示に紐付く工程実績一覧を取得する（WO-003）。</summary>
public record GetWorkOrderOperationsQuery(Guid WorkOrderId) : IRequest<List<WorkOrderOperationDto>>;

public class GetWorkOrderOperationsQueryHandler
    : IRequestHandler<GetWorkOrderOperationsQuery, List<WorkOrderOperationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrderOperationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkOrderOperationDto>> Handle(
        GetWorkOrderOperationsQuery request,
        CancellationToken cancellationToken)
    {
        var workOrderExists = await _context.WorkOrders
            .AnyAsync(w => w.Id == request.WorkOrderId, cancellationToken);
        if (!workOrderExists)
            throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        return await _context.WorkOrderOperations
            .Include(o => o.WorkOrder)
            .Include(o => o.Process)
            .Include(o => o.Equipment)
            .Where(o => o.WorkOrderId == request.WorkOrderId)
            .OrderBy(o => o.Sequence)
            .Select(o => new WorkOrderOperationDto(
                o.Id, o.WorkOrderId, o.WorkOrder.WorkOrderNumber,
                o.Sequence, o.ProcessId, o.Process.Code, o.Process.Name,
                o.EquipmentId, o.Equipment != null ? o.Equipment.Name : null,
                o.Status, o.ActualStartAt, o.ActualEndAt,
                o.ActualQuantity, o.DefectQuantity,
                o.WorkerUserId, o.Notes,
                o.CreatedAt, o.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
