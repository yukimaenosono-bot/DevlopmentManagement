using Synapse.Domain.Enums;

namespace Synapse.Application.Items.Dtos;

/// <summary>
/// 品目マスタの参照用DTO。一覧・詳細の両方で使用する。
/// フロントエンドの型定義（ItemDto）と 1:1 で対応させること。
/// </summary>
public record ItemDto(
    Guid Id,
    string Code,
    string Name,
    string? ShortName,
    ItemType ItemType,
    string Unit,
    decimal StandardUnitPrice,
    decimal SafetyStockQuantity,
    bool HasExpirationDate,
    bool IsLotManaged,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
