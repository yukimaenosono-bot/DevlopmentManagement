using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Routings.Commands;

/// <summary>
/// ルーティングを更新する。ステップは全件置き換え（差分ではなく洗い替え）。
/// </summary>
public record UpdateRoutingCommand(
    Guid Id,
    string Name,
    bool IsDefault,
    IEnumerable<UpdateRoutingStepRequest> Steps
) : IRequest;

public record UpdateRoutingStepRequest(
    int Sequence,
    Guid ProcessId,
    Guid? EquipmentId,
    decimal? StandardTime,
    bool IsRequired
);

public class UpdateRoutingCommandHandler : IRequestHandler<UpdateRoutingCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoutingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateRoutingCommand request, CancellationToken cancellationToken)
    {
        var routing = await _context.Routings
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Routing), request.Id);

        // IsDefault=true の場合、同品目の既存デフォルトを解除する
        if (request.IsDefault && !routing.IsDefault)
        {
            var others = await _context.Routings
                .Where(r => r.ItemId == routing.ItemId && r.IsDefault && r.Id != request.Id)
                .ToListAsync(cancellationToken);
            foreach (var r in others)
                r.Update(r.Name, isDefault: false);
        }

        routing.Update(request.Name, request.IsDefault);

        // ステップを洗い替え: 既存を削除して再登録
        var oldSteps = routing.Steps.ToList();
        foreach (var step in oldSteps)
            _context.RoutingSteps.Remove(step);

        foreach (var step in request.Steps)
        {
            var newStep = RoutingStep.Create(
                routing.Id, step.Sequence, step.ProcessId,
                step.EquipmentId, step.StandardTime, step.IsRequired);
            _context.RoutingSteps.Add(newStep);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
