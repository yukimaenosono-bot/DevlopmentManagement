using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ShipmentOrders.Commands;

/// <summary>ピッキング完了に移行する（SH-003）。</summary>
public record MarkShipmentPickedCommand(Guid Id) : IRequest;

public class MarkShipmentPickedCommandHandler : IRequestHandler<MarkShipmentPickedCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkShipmentPickedCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarkShipmentPickedCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.ShipmentOrders
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ShipmentOrder), request.Id);

        order.MarkAsPicked();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
