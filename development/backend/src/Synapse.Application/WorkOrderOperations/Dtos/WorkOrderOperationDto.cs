using Synapse.Domain.Enums;

namespace Synapse.Application.WorkOrderOperations.Dtos;

/// <summary>工程実績の参照用 DTO。</summary>
public record WorkOrderOperationDto(
    Guid Id,
    Guid WorkOrderId,
    string WorkOrderNumber,
    int Sequence,
    Guid ProcessId,
    string ProcessCode,
    string ProcessName,
    Guid? EquipmentId,
    string? EquipmentName,
    WorkOrderOperationStatus Status,
    DateTime? ActualStartAt,
    DateTime? ActualEndAt,
    decimal? ActualQuantity,
    decimal? DefectQuantity,
    string? WorkerUserId,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
