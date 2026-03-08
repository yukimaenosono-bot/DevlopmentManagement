using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.ProductionPlans.Commands;

/// <summary>生産計画を確定する（PP-001）。確定後は製造指示への展開が可能になる。</summary>
public record ConfirmProductionPlanCommand(Guid Id) : IRequest;

public class ConfirmProductionPlanCommandHandler : IRequestHandler<ConfirmProductionPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public ConfirmProductionPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ConfirmProductionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.ProductionPlans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ProductionPlan), request.Id);

        plan.Confirm();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
