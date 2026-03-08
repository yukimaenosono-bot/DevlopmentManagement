using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.QualityInspections.Dtos;
using Synapse.Domain.Enums;

namespace Synapse.Application.QualityInspections.Queries;

/// <summary>品質検査一覧を取得する（QC-001〜003）。</summary>
public record GetQualityInspectionListQuery(
    InspectionType? InspectionType,
    Guid? ItemId,
    InspectionResult? Result,
    DateOnly? From,
    DateOnly? To
) : IRequest<List<QualityInspectionDto>>;

public class GetQualityInspectionListQueryHandler
    : IRequestHandler<GetQualityInspectionListQuery, List<QualityInspectionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetQualityInspectionListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<QualityInspectionDto>> Handle(
        GetQualityInspectionListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.QualityInspections
            .Include(q => q.Item)
            .Include(q => q.WorkOrder)
            .AsQueryable();

        if (request.InspectionType.HasValue)
            query = query.Where(q => q.InspectionType == request.InspectionType.Value);

        if (request.ItemId.HasValue)
            query = query.Where(q => q.ItemId == request.ItemId.Value);

        if (request.Result.HasValue)
            query = query.Where(q => q.Result == request.Result.Value);

        if (request.From.HasValue)
            query = query.Where(q => DateOnly.FromDateTime(q.InspectedAt) >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(q => DateOnly.FromDateTime(q.InspectedAt) <= request.To.Value);

        return await query
            .OrderByDescending(q => q.InspectedAt)
            .Select(q => new QualityInspectionDto(
                q.Id, q.InspectionNumber,
                q.InspectionType,
                q.ItemId, q.Item.Code, q.Item.Name,
                q.LotNumber,
                q.WorkOrderId, q.WorkOrder != null ? q.WorkOrder.WorkOrderNumber : null,
                q.InspectedAt,
                q.InspectorUserId,
                q.InspectionQuantity, q.PassQuantity, q.FailQuantity,
                q.Result, q.Notes,
                q.CreatedAt, q.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
