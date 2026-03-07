namespace Synapse.Application.Bom.Dtos;

/// <summary>
/// BOM（部品表）1行の参照用 DTO。
/// 子品目名は一覧表示に必要なため ChildItemCode / ChildItemName を含める。
/// フロントエンドの型定義（BomLineDto）と 1:1 で対応させること。
/// </summary>
public record BomLineDto(
    Guid ParentItemId,
    Guid ChildItemId,
    string ChildItemCode,
    string ChildItemName,
    decimal Quantity,
    string Unit,
    DateOnly ValidFrom,
    DateOnly? ValidTo
);
