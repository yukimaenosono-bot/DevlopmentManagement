using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrders.Commands;

/// <summary>製造指示をキャンセルするコマンド。発行済・着手中のみ可能。</summary>
public record CancelWorkOrderCommand(Guid Id) : IRequest;

public class CancelWorkOrderCommandHandler : IRequestHandler<CancelWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelWorkOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        // キャンセル可否チェックはエンティティ側で担保する
        workOrder.Cancel();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
