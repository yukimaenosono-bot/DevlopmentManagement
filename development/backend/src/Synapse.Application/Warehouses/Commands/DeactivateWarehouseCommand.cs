using MediatR;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Warehouses.Commands;

/// <summary>倉庫を廃止する（論理削除）コマンド。</summary>
public record DeactivateWarehouseCommand(Guid Id) : IRequest;

public class DeactivateWarehouseCommandHandler : IRequestHandler<DeactivateWarehouseCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateWarehouseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Warehouse), request.Id);

        warehouse.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
