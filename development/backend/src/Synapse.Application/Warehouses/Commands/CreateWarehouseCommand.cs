using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;

namespace Synapse.Application.Warehouses.Commands;

/// <summary>倉庫マスタを新規作成するコマンド。</summary>
public record CreateWarehouseCommand(string Code, string Name, WarehouseType WarehouseType) : IRequest<Guid>;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateWarehouseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Warehouses
            .AnyAsync(w => w.Code == request.Code, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"倉庫コード「{request.Code}」は既に登録されています。");

        var warehouse = Warehouse.Create(request.Code, request.Name, request.WarehouseType);
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        return warehouse.Id;
    }
}
