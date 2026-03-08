using Synapse.Domain.Enums;

namespace Synapse.Application.ShipmentOrders.Dtos;

public record ShipmentOrderDto(
    Guid Id,
    string ShipmentNumber,
    string OrderReference,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal PlannedQuantity,
    DateOnly PlannedShipDate,
    string? LotNumber,
    string? Carrier,
    string? Notes,
    ShipmentOrderStatus Status,
    DateTime? ShippedAt,
    decimal? ActualQuantity,
    string? ShippedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
