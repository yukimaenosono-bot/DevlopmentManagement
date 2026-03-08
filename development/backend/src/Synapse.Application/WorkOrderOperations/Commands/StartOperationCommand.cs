using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrderOperations.Commands;

/// <summary>工程を着手中に移行する（WO-001）。</summary>
public record StartOperationCommand(
    Guid OperationId,
    string WorkerUserId,
    DateTime StartAt
) : IRequest;

public class StartOperationCommandHandler : IRequestHandler<StartOperationCommand>
{
    private readonly IApplicationDbContext _context;

    public StartOperationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StartOperationCommand request, CancellationToken cancellationToken)
    {
        var op = await _context.WorkOrderOperations
            .FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderOperation), request.OperationId);

        op.Start(request.WorkerUserId, request.StartAt);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
