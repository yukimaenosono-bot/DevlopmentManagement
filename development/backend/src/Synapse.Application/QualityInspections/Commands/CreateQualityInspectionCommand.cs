using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.QualityInspections.Commands;

/// <summary>品質検査を登録する（QC-001〜003）。戻り値は生成された検査の ID。</summary>
public record CreateQualityInspectionCommand(
    InspectionType InspectionType,
    Guid ItemId,
    string LotNumber,
    Guid? WorkOrderId,
    DateTime InspectedAt,
    string InspectorUserId,
    decimal InspectionQuantity,
    decimal PassQuantity,
    decimal FailQuantity,
    InspectionResult Result,
    string? Notes
) : IRequest<Guid>;

public class CreateQualityInspectionCommandHandler : IRequestHandler<CreateQualityInspectionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateQualityInspectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateQualityInspectionCommand request, CancellationToken cancellationToken)
    {
        var itemExists = await _context.Items
            .AnyAsync(i => i.Id == request.ItemId && i.IsActive, cancellationToken);
        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        if (request.WorkOrderId.HasValue)
        {
            var woExists = await _context.WorkOrders
                .AnyAsync(w => w.Id == request.WorkOrderId.Value, cancellationToken);
            if (!woExists)
                throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId.Value);
        }

        // 検査番号を採番する: QI-YYYYMMDD-NNNN
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"QI-{today}-";
        var todayCount = await _context.QualityInspections
            .CountAsync(q => q.InspectionNumber.StartsWith(prefix), cancellationToken);
        var inspectionNumber = $"{prefix}{todayCount + 1:0000}";

        var inspection = QualityInspection.Create(
            inspectionNumber,
            request.InspectionType,
            request.ItemId,
            request.LotNumber,
            request.WorkOrderId,
            request.InspectedAt,
            request.InspectorUserId,
            request.InspectionQuantity,
            request.PassQuantity,
            request.FailQuantity,
            request.Result,
            request.Notes);

        _context.QualityInspections.Add(inspection);
        await _context.SaveChangesAsync(cancellationToken);

        return inspection.Id;
    }
}
