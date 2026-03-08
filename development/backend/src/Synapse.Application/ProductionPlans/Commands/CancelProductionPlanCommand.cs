using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ProductionPlans.Commands;

/// <summary>生産計画をキャンセルする。完了済みはキャンセル不可。</summary>
public record CancelProductionPlanCommand(Guid Id) : IRequest;

public class CancelProductionPlanCommandHandler : IRequestHandler<CancelProductionPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelProductionPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelProductionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.ProductionPlans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductionPlan), request.Id);

        plan.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
