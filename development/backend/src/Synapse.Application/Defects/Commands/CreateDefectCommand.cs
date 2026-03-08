using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Defects.Commands;

/// <summary>不良を登録する（QC-004）。戻り値は生成された不良の ID。</summary>
public record CreateDefectCommand(
    Guid? QualityInspectionId,
    Guid? WorkOrderId,
    Guid ItemId,
    DateTime OccurredAt,
    Guid? ProcessId,
    DefectCategory Category,
    string Description,
    decimal Quantity,
    string? EstimatedCause,
    string? CorrectiveAction,
    DispositionType Disposition,
    string? DispositionNote
) : IRequest<Guid>;

public class CreateDefectCommandHandler : IRequestHandler<CreateDefectCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDefectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDefectCommand request, CancellationToken cancellationToken)
    {
        var itemExists = await _context.Items
            .AnyAsync(i => i.Id == request.ItemId && i.IsActive, cancellationToken);
        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        // 不良番号を採番する: DF-YYYYMMDD-NNNN
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"DF-{today}-";
        var todayCount = await _context.Defects
            .CountAsync(d => d.DefectNumber.StartsWith(prefix), cancellationToken);
        var defectNumber = $"{prefix}{todayCount + 1:0000}";

        var defect = Defect.Create(
            defectNumber,
            request.QualityInspectionId,
            request.WorkOrderId,
            request.ItemId,
            request.OccurredAt,
            request.ProcessId,
            request.Category,
            request.Description,
            request.Quantity,
            request.EstimatedCause,
            request.CorrectiveAction,
            request.Disposition,
            request.DispositionNote);

        _context.Defects.Add(defect);
        await _context.SaveChangesAsync(cancellationToken);

        return defect.Id;
    }
}
