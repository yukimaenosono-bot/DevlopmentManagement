using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrderOperations.Commands;

/// <summary>工程を完了に移行し、実績を記録する（WO-002）。</summary>
public record CompleteOperationCommand(
    Guid OperationId,
    decimal ActualQuantity,
    decimal DefectQuantity,
    DateTime EndAt,
    string? Notes
) : IRequest;

public class CompleteOperationCommandHandler : IRequestHandler<CompleteOperationCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteOperationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CompleteOperationCommand request, CancellationToken cancellationToken)
    {
        var op = await _context.WorkOrderOperations
            .FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrderOperation), request.OperationId);

        op.Complete(request.ActualQuantity, request.DefectQuantity, request.EndAt, request.Notes);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
