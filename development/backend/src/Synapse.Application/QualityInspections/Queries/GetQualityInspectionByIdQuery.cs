using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.QualityInspections.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.QualityInspections.Queries;

/// <summary>品質検査を1件取得する。</summary>
public record GetQualityInspectionByIdQuery(Guid Id) : IRequest<QualityInspectionDto>;

public class GetQualityInspectionByIdQueryHandler
    : IRequestHandler<GetQualityInspectionByIdQuery, QualityInspectionDto>
{
    private readonly IApplicationDbContext _context;

    public GetQualityInspectionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QualityInspectionDto> Handle(
        GetQualityInspectionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var q = await _context.QualityInspections
            .Include(x => x.Item)
            .Include(x => x.WorkOrder)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(QualityInspection), request.Id);

        return new QualityInspectionDto(
            q.Id, q.InspectionNumber,
            q.InspectionType,
            q.ItemId, q.Item.Code, q.Item.Name,
            q.LotNumber,
            q.WorkOrderId, q.WorkOrder?.WorkOrderNumber,
            q.InspectedAt,
            q.InspectorUserId,
            q.InspectionQuantity, q.PassQuantity, q.FailQuantity,
            q.Result, q.Notes,
            q.CreatedAt, q.UpdatedAt);
    }
}
