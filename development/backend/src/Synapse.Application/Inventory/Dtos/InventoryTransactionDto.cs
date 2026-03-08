using Synapse.Domain.Enums;

namespace Synapse.Application.Inventory.Dtos;

/// <summary>入出庫履歴の参照用 DTO。</summary>
public record InventoryTransactionDto(
    Guid Id,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    string? LotNumber,
    InventoryTransactionType TransactionType,
    decimal Quantity,
    string? ReferenceNumber,
    string? Note,
    DateTime TransactedAt,
    string CreatedByUserId);
