using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Routings.Dtos;

namespace Synapse.Application.Routings.Queries;

/// <summary>ルーティング一覧を取得する。itemId で品目を絞り込める。</summary>
public record GetRoutingListQuery(
    Guid? ItemId = null,
    bool ActiveOnly = true
) : IRequest<List<RoutingDto>>;

public class GetRoutingListQueryHandler : IRequestHandler<GetRoutingListQuery, List<RoutingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoutingListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoutingDto>> Handle(GetRoutingListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Routings
            .Include(r => r.Item)
            .Include(r => r.Steps)
                .ThenInclude(s => s.Process)
            .Include(r => r.Steps)
                .ThenInclude(s => s.Equipment)
            .AsQueryable();

        if (request.ItemId.HasValue)
            query = query.Where(r => r.ItemId == request.ItemId.Value);

        if (request.ActiveOnly)
            query = query.Where(r => r.IsActive);

        return await query
            .OrderBy(r => r.Item.Code)
            .ThenByDescending(r => r.IsDefault)
            .Select(r => new RoutingDto(
                r.Id, r.ItemId, r.Item.Code, r.Item.Name,
                r.Name, r.IsDefault, r.IsActive,
                r.Steps.OrderBy(s => s.Sequence).Select(s => new RoutingStepDto(
                    s.Id, s.Sequence,
                    s.ProcessId, s.Process.Code, s.Process.Name,
                    s.EquipmentId, s.Equipment != null ? s.Equipment.Name : null,
                    s.StandardTime, s.IsRequired)),
                r.CreatedAt, r.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
