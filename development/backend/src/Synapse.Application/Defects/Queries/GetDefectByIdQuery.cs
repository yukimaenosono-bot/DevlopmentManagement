using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Defects.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Defects.Queries;

/// <summary>不良を1件取得する。</summary>
public record GetDefectByIdQuery(Guid Id) : IRequest<DefectDto>;

public class GetDefectByIdQueryHandler : IRequestHandler<GetDefectByIdQuery, DefectDto>
{
    private readonly IApplicationDbContext _context;

    public GetDefectByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DefectDto> Handle(GetDefectByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await _context.Defects
            .Include(x => x.Item)
            .Include(x => x.WorkOrder)
            .Include(x => x.Process)
            .Include(x => x.QualityInspection)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Defect), request.Id);

        return new DefectDto(
            d.Id, d.DefectNumber,
            d.QualityInspectionId,
            d.QualityInspection?.InspectionNumber,
            d.WorkOrderId,
            d.WorkOrder?.WorkOrderNumber,
            d.ItemId, d.Item.Code, d.Item.Name,
            d.OccurredAt,
            d.ProcessId,
            d.Process?.Code,
            d.Process?.Name,
            d.Category, d.Description, d.Quantity,
            d.EstimatedCause, d.CorrectiveAction,
            d.Disposition, d.DispositionNote,
            d.CreatedAt, d.UpdatedAt);
    }
}
