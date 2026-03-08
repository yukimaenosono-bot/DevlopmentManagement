using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Warehouses.Dtos;

namespace Synapse.Application.Warehouses.Queries;

/// <summary>倉庫マスタ一覧を取得するクエリ。</summary>
public record GetWarehouseListQuery(bool ActiveOnly = true) : IRequest<List<WarehouseDto>>;

public class GetWarehouseListQueryHandler : IRequestHandler<GetWarehouseListQuery, List<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWarehouseListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarehouseDto>> Handle(GetWarehouseListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Warehouses.AsQueryable();

        if (request.ActiveOnly)
            query = query.Where(w => w.IsActive);

        return await query
            .OrderBy(w => w.Code)
            .Select(w => new WarehouseDto(w.Id, w.Code, w.Name, w.WarehouseType, w.IsActive))
            .ToListAsync(cancellationToken);
    }
}
