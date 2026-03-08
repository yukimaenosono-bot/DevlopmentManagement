namespace Synapse.Application.Routings.Dtos;

/// <summary>ルーティングの参照用 DTO。</summary>
public record RoutingDto(
    Guid Id,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string Name,
    bool IsDefault,
    bool IsActive,
    IEnumerable<RoutingStepDto> Steps,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>ルーティングステップの参照用 DTO。</summary>
public record RoutingStepDto(
    Guid Id,
    int Sequence,
    Guid ProcessId,
    string ProcessCode,
    string ProcessName,
    Guid? EquipmentId,
    string? EquipmentName,
    decimal? StandardTime,
    bool IsRequired
);
