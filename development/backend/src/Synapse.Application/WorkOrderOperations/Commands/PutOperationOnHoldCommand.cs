using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrderOperations.Commands;

/// <summary>工程を保留にする。</summary>
public record PutOperationOnHoldCommand(Guid OperationId) : IRequest;

public class PutOperationOnHoldCommandHandler : IRequestHandler<PutOperationOnHoldCommand>
{
    private readonly IApplicationDbContext _context;

    public PutOperationOnHoldCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PutOperationOnHoldCommand request, CancellationToken cancellationToken)
    {
        var op = await _context.WorkOrderOperations
            .FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderOperation), request.OperationId);

        op.PutOnHold();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
