using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Routings.Dtos;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Routings.Queries;

/// <summary>ルーティングを1件取得する（ステップ含む）。</summary>
public record GetRoutingByIdQuery(Guid Id) : IRequest<RoutingDto>;

public class GetRoutingByIdQueryHandler : IRequestHandler<GetRoutingByIdQuery, RoutingDto>
{
    private readonly IApplicationDbContext _context;

    public GetRoutingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoutingDto> Handle(GetRoutingByIdQuery request, CancellationToken cancellationToken)
    {
        var routing = await _context.Routings
            .Include(r => r.Item)
            .Include(r => r.Steps)
                .ThenInclude(s => s.Process)
            .Include(r => r.Steps)
                .ThenInclude(s => s.Equipment)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Synapse.Domain.Entities.Routing), request.Id);

        return new RoutingDto(
            routing.Id, routing.ItemId, routing.Item.Code, routing.Item.Name,
            routing.Name, routing.IsDefault, routing.IsActive,
            routing.Steps.OrderBy(s => s.Sequence).Select(s => new RoutingStepDto(
                s.Id, s.Sequence,
                s.ProcessId, s.Process.Code, s.Process.Name,
                s.EquipmentId, s.Equipment?.Name,
                s.StandardTime, s.IsRequired)),
            routing.CreatedAt, routing.UpdatedAt);
    }
}
