using Synapse.Domain.Enums;

namespace Synapse.Application.QualityInspections.Dtos;

public record QualityInspectionDto(
    Guid Id,
    string InspectionNumber,
    InspectionType InspectionType,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string LotNumber,
    Guid? WorkOrderId,
    string? WorkOrderNumber,
    DateTime InspectedAt,
    string InspectorUserId,
    decimal InspectionQuantity,
    decimal PassQuantity,
    decimal FailQuantity,
    InspectionResult Result,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
