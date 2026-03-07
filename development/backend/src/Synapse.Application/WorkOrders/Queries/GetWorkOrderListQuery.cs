using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.WorkOrders.Dtos;
using Synapse.Domain.Enums;

namespace Synapse.Application.WorkOrders.Queries;

/// <summary>
/// 製造指示一覧を取得する。
/// ステータスや日付でフィルタリング可能（製造指示一覧画面 SCR-MO-001 での検索用途）。
/// </summary>
public record GetWorkOrderListQuery(
    WorkOrderStatus? Status = null,
    DateOnly? From = null,
    DateOnly? To = null
) : IRequest<List<WorkOrderDto>>;

public class GetWorkOrderListQueryHandler : IRequestHandler<GetWorkOrderListQuery, List<WorkOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrderListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkOrderDto>> Handle(GetWorkOrderListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkOrders
            // 品目名を一覧に表示するために Item を JOIN する
            .Include(w => w.Item)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(w => w.Status == request.Status.Value);

        // 製造開始予定日での絞り込み（製造指示一覧の日付範囲検索）
        if (request.From.HasValue)
            query = query.Where(w => w.PlannedStartDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(w => w.PlannedStartDate <= request.To.Value);

        // 新しい発行順で表示する（製造指示番号は採番日時順）
        return await query
            .OrderByDescending(w => w.WorkOrderNumber)
            .Select(w => new WorkOrderDto(
                w.Id, w.WorkOrderNumber, w.ProductionPlanNumber,
                w.ItemId, w.Item.Code, w.Item.Name,
                w.LotNumber, w.Quantity,
                w.PlannedStartDate, w.PlannedEndDate,
                w.Status, w.WorkInstruction, w.CreatedByUserId,
                w.CreatedAt, w.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
