using Synapse.Domain.Enums;

namespace Synapse.Application.Defects.Dtos;

public record DefectDto(
    Guid Id,
    string DefectNumber,
    Guid? QualityInspectionId,
    string? InspectionNumber,
    Guid? WorkOrderId,
    string? WorkOrderNumber,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    DateTime OccurredAt,
    Guid? ProcessId,
    string? ProcessCode,
    string? ProcessName,
    DefectCategory Category,
    string Description,
    decimal Quantity,
    string? EstimatedCause,
    string? CorrectiveAction,
    DispositionType Disposition,
    string? DispositionNote,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
