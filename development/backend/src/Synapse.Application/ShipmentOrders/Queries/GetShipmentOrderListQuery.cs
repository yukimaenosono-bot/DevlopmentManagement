using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.ShipmentOrders.Dtos;
using Synapse.Domain.Enums;

namespace Synapse.Application.ShipmentOrders.Queries;

/// <summary>出荷指示一覧を取得する（SH-002）。</summary>
public record GetShipmentOrderListQuery(
    ShipmentOrderStatus? Status,
    Guid? CustomerId,
    DateOnly? From,
    DateOnly? To
) : IRequest<List<ShipmentOrderDto>>;

public class GetShipmentOrderListQueryHandler
    : IRequestHandler<GetShipmentOrderListQuery, List<ShipmentOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetShipmentOrderListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShipmentOrderDto>> Handle(
        GetShipmentOrderListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.ShipmentOrders
            .Include(s => s.Customer)
            .Include(s => s.Item)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(s => s.CustomerId == request.CustomerId.Value);

        if (request.From.HasValue)
            query = query.Where(s => s.PlannedShipDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(s => s.PlannedShipDate <= request.To.Value);

        return await query
            .OrderByDescending(s => s.PlannedShipDate)
            .Select(s => new ShipmentOrderDto(
                s.Id, s.ShipmentNumber, s.OrderReference,
                s.CustomerId, s.Customer.Code, s.Customer.Name,
                s.ItemId, s.Item.Code, s.Item.Name,
                s.PlannedQuantity, s.PlannedShipDate,
                s.LotNumber, s.Carrier, s.Notes,
                s.Status, s.ShippedAt, s.ActualQuantity, s.ShippedByUserId,
                s.CreatedAt, s.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
