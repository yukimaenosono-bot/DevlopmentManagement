using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrderOperations.Commands;

/// <summary>
/// 製造指示に対してルーティングから工程実績を生成する。
/// ルーティング ID を省略した場合は品目のデフォルトルーティングを使用する。
/// </summary>
public record GenerateOperationsCommand(
    Guid WorkOrderId,
    Guid? RoutingId = null
) : IRequest;

public class GenerateOperationsCommandHandler : IRequestHandler<GenerateOperationsCommand>
{
    private readonly IApplicationDbContext _context;

    public GenerateOperationsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(GenerateOperationsCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        Routing? routing;
        if (request.RoutingId.HasValue)
        {
            routing = await _context.Routings
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == request.RoutingId.Value && r.IsActive, cancellationToken)
                ?? throw new NotFoundException(nameof(Routing), request.RoutingId.Value);
        }
        else
        {
            routing = await _context.Routings
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.ItemId == workOrder.ItemId && r.IsDefault && r.IsActive, cancellationToken);

            // デフォルトルーティングが存在しない場合はスキップ（工程なし製造指示を許容）
            if (routing == null) return;
        }

        foreach (var step in routing.Steps.OrderBy(s => s.Sequence))
        {
            var op = WorkOrderOperation.Create(
                workOrder.Id, step.Sequence, step.ProcessId, step.EquipmentId);
            _context.WorkOrderOperations.Add(op);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
