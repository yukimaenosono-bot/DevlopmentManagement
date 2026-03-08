using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ShipmentOrders.Commands;

/// <summary>出荷指示を新規作成する（SH-001）。戻り値は生成された出荷指示の ID。</summary>
public record CreateShipmentOrderCommand(
    string OrderReference,
    Guid CustomerId,
    Guid ItemId,
    decimal PlannedQuantity,
    DateOnly PlannedShipDate,
    string? LotNumber,
    string? Carrier,
    string? Notes
) : IRequest<Guid>;

public class CreateShipmentOrderCommandHandler : IRequestHandler<CreateShipmentOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateShipmentOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateShipmentOrderCommand request, CancellationToken cancellationToken)
    {
        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == request.CustomerId && c.IsActive, cancellationToken);
        if (!customerExists)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        var itemExists = await _context.Items
            .AnyAsync(i => i.Id == request.ItemId && i.IsActive, cancellationToken);
        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        // 出荷指示番号を採番する: SH-YYYYMMDD-NNNN
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"SH-{today}-";
        var todayCount = await _context.ShipmentOrders
            .CountAsync(s => s.ShipmentNumber.StartsWith(prefix), cancellationToken);
        var shipmentNumber = $"{prefix}{todayCount + 1:0000}";

        var order = ShipmentOrder.Create(
            shipmentNumber,
            request.OrderReference,
            request.CustomerId,
            request.ItemId,
            request.PlannedQuantity,
            request.PlannedShipDate,
            request.LotNumber,
            request.Carrier,
            request.Notes);

        _context.ShipmentOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
