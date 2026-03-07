namespace Synapse.Application.Equipments.Dtos;

/// <summary>
/// 設備マスタの参照用 DTO。一覧・詳細の両方で使用する。
/// 画面表示で工程名が必要なため ProcessCode / ProcessName を含める。
/// フロントエンドの型定義（EquipmentDto）と 1:1 で対応させること。
/// </summary>
public record EquipmentDto(
    Guid Id,
    string Code,
    string Name,
    Guid ProcessId,
    string ProcessCode,
    string ProcessName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
