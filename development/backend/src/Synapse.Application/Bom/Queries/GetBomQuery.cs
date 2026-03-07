using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Bom.Dtos;
using Synapse.Application.Common.Interfaces;

namespace Synapse.Application.Bom.Queries;

/// <summary>
/// 指定した親品目の BOM（部品表）を取得する。
/// asOf を指定すると有効なラインのみ返す（製造指示発行時の引当計算で使用）。
/// </summary>
public record GetBomQuery(
    Guid ParentItemId,
    DateOnly? AsOf = null
) : IRequest<List<BomLineDto>>;

public class GetBomQueryHandler : IRequestHandler<GetBomQuery, List<BomLineDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBomQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BomLineDto>> Handle(GetBomQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BomLines
            .Include(b => b.ChildItem)
            .Where(b => b.ParentItemId == request.ParentItemId);

        // 有効日指定がある場合は有効期間内のラインのみ返す（製造指示発行時の引当計算用）
        if (request.AsOf.HasValue)
        {
            var asOf = request.AsOf.Value;
            query = query.Where(b =>
                b.ValidFrom <= asOf &&
                (b.ValidTo == null || b.ValidTo >= asOf));
        }

        return await query
            .OrderBy(b => b.ChildItem.Code)
            .Select(b => new BomLineDto(
                b.ParentItemId, b.ChildItemId,
                b.ChildItem.Code, b.ChildItem.Name,
                b.Quantity, b.Unit,
                b.ValidFrom, b.ValidTo))
            .ToListAsync(cancellationToken);
    }
}
