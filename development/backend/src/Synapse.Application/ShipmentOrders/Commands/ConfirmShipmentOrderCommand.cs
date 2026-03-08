using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ShipmentOrders.Commands;

/// <summary>出荷指示を確定する（出荷待ち）。</summary>
public record ConfirmShipmentOrderCommand(Guid Id) : IRequest;

public class ConfirmShipmentOrderCommandHandler : IRequestHandler<ConfirmShipmentOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public ConfirmShipmentOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ConfirmShipmentOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.ShipmentOrders
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ShipmentOrder), request.Id);

        order.Confirm();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
