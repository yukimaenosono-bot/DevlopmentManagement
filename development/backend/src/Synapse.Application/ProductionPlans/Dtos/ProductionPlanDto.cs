using Synapse.Domain.Enums;

namespace Synapse.Application.ProductionPlans.Dtos;

/// <summary>生産計画の参照用 DTO。一覧・詳細の両方で使用する。</summary>
public record ProductionPlanDto(
    Guid Id,
    string PlanNumber,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    decimal PlannedQuantity,
    DateOnly PlanStartDate,
    DateOnly PlanEndDate,
    DateOnly DueDate,
    ProductionPlanStatus Status,
    PlanPriority Priority,
    string? Notes,
    string? OrderReference,
    string CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
