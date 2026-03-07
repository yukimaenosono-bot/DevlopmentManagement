using Synapse.Domain.Enums;

namespace Synapse.Application.WorkOrders.Dtos;

/// <summary>
/// 製造指示の参照用 DTO。一覧・詳細の両方で使用する。
/// 品目名は一覧表示に必要なため ItemCode / ItemName を含める。
/// フロントエンドの型定義（WorkOrderDto）と 1:1 で対応させること。
/// </summary>
public record WorkOrderDto(
    Guid Id,
    string WorkOrderNumber,
    string? ProductionPlanNumber,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string? LotNumber,
    decimal Quantity,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    WorkOrderStatus Status,
    string? WorkInstruction,
    string CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
