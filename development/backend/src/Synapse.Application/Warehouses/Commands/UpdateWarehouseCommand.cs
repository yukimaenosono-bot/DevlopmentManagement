using MediatR;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Warehouses.Commands;

/// <summary>倉庫名・倉庫区分を更新するコマンド。</summary>
public record UpdateWarehouseCommand(Guid Id, string Name, WarehouseType WarehouseType) : IRequest;

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateWarehouseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Warehouse), request.Id);

        warehouse.Update(request.Name, request.WarehouseType);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
