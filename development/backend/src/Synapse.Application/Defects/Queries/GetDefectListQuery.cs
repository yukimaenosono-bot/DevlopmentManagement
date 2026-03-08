using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Defects.Dtos;
using Synapse.Domain.Enums;

namespace Synapse.Application.Defects.Queries;

/// <summary>不良一覧を取得する（QC-004）。</summary>
public record GetDefectListQuery(
    Guid? ItemId,
    Guid? WorkOrderId,
    DefectCategory? Category,
    DispositionType? Disposition,
    DateOnly? From,
    DateOnly? To
) : IRequest<List<DefectDto>>;

public class GetDefectListQueryHandler
    : IRequestHandler<GetDefectListQuery, List<DefectDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDefectListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DefectDto>> Handle(
        GetDefectListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Defects
            .Include(d => d.Item)
            .Include(d => d.WorkOrder)
            .Include(d => d.Process)
            .Include(d => d.QualityInspection)
            .AsQueryable();

        if (request.ItemId.HasValue)
            query = query.Where(d => d.ItemId == request.ItemId.Value);

        if (request.WorkOrderId.HasValue)
            query = query.Where(d => d.WorkOrderId == request.WorkOrderId.Value);

        if (request.Category.HasValue)
            query = query.Where(d => d.Category == request.Category.Value);

        if (request.Disposition.HasValue)
            query = query.Where(d => d.Disposition == request.Disposition.Value);

        if (request.From.HasValue)
            query = query.Where(d => DateOnly.FromDateTime(d.OccurredAt) >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(d => DateOnly.FromDateTime(d.OccurredAt) <= request.To.Value);

        return await query
            .OrderByDescending(d => d.OccurredAt)
            .Select(d => new DefectDto(
                d.Id, d.DefectNumber,
                d.QualityInspectionId,
                d.QualityInspection != null ? d.QualityInspection.InspectionNumber : null,
                d.WorkOrderId,
                d.WorkOrder != null ? d.WorkOrder.WorkOrderNumber : null,
                d.ItemId, d.Item.Code, d.Item.Name,
                d.OccurredAt,
                d.ProcessId,
                d.Process != null ? d.Process.Code : null,
                d.Process != null ? d.Process.Name : null,
                d.Category, d.Description, d.Quantity,
                d.EstimatedCause, d.CorrectiveAction,
                d.Disposition, d.DispositionNote,
                d.CreatedAt, d.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
