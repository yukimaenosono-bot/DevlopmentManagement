using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Routings.Commands;

/// <summary>ルーティングとそのステップを作成する。</summary>
public record CreateRoutingCommand(
    Guid ItemId,
    string Name,
    bool IsDefault,
    IEnumerable<CreateRoutingStepRequest> Steps
) : IRequest<Guid>;

public record CreateRoutingStepRequest(
    int Sequence,
    Guid ProcessId,
    Guid? EquipmentId,
    decimal? StandardTime,
    bool IsRequired
);

public class CreateRoutingCommandHandler : IRequestHandler<CreateRoutingCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateRoutingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRoutingCommand request, CancellationToken cancellationToken)
    {
        var itemExists = await _context.Items
            .AnyAsync(i => i.Id == request.ItemId && i.IsActive, cancellationToken);
        if (!itemExists)
            throw new NotFoundException(nameof(Item), request.ItemId);

        var routing = Routing.Create(request.ItemId, request.Name, request.IsDefault);

        // IsDefault=true の場合、同品目の既存デフォルトを解除する
        if (request.IsDefault)
        {
            var existing = await _context.Routings
                .Where(r => r.ItemId == request.ItemId && r.IsDefault)
                .ToListAsync(cancellationToken);
            foreach (var r in existing)
                r.Update(r.Name, isDefault: false);
        }

        _context.Routings.Add(routing);
        await _context.SaveChangesAsync(cancellationToken);

        // ステップを追加
        foreach (var step in request.Steps)
        {
            var routingStep = RoutingStep.Create(
                routing.Id, step.Sequence, step.ProcessId,
                step.EquipmentId, step.StandardTime, step.IsRequired);
            _context.RoutingSteps.Add(routingStep);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return routing.Id;
    }
}
