using Synapse.Domain.Enums;

namespace Synapse.Application.Warehouses.Dtos;

/// <summary>倉庫マスタの参照用 DTO。</summary>
public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    WarehouseType WarehouseType,
    bool IsActive);
