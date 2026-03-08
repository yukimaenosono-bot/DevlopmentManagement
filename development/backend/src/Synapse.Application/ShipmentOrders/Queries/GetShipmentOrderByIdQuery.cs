using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.ShipmentOrders.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ShipmentOrders.Queries;

/// <summary>出荷指示を1件取得する。</summary>
public record GetShipmentOrderByIdQuery(Guid Id) : IRequest<ShipmentOrderDto>;

public class GetShipmentOrderByIdQueryHandler
    : IRequestHandler<GetShipmentOrderByIdQuery, ShipmentOrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetShipmentOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShipmentOrderDto> Handle(
        GetShipmentOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var s = await _context.ShipmentOrders
            .Include(x => x.Customer)
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ShipmentOrder), request.Id);

        return new ShipmentOrderDto(
            s.Id, s.ShipmentNumber, s.OrderReference,
            s.CustomerId, s.Customer.Code, s.Customer.Name,
            s.ItemId, s.Item.Code, s.Item.Name,
            s.PlannedQuantity, s.PlannedShipDate,
            s.LotNumber, s.Carrier, s.Notes,
            s.Status, s.ShippedAt, s.ActualQuantity, s.ShippedByUserId,
            s.CreatedAt, s.UpdatedAt);
    }
}
