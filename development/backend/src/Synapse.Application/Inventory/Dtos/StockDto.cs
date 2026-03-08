namespace Synapse.Application.Inventory.Dtos;

/// <summary>在庫照会の参照用 DTO。</summary>
public record StockDto(
    Guid Id,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string ItemUnit,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? LotNumber,
    decimal Quantity,
    DateTime LastTransactedAt);
