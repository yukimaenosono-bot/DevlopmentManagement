using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ShipmentOrders.Commands;

/// <summary>出荷指示をキャンセルする。出荷済みはキャンセル不可。</summary>
public record CancelShipmentOrderCommand(Guid Id) : IRequest;

public class CancelShipmentOrderCommandHandler : IRequestHandler<CancelShipmentOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelShipmentOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelShipmentOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.ShipmentOrders
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ShipmentOrder), request.Id);

        order.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
