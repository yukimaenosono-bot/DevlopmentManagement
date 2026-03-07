using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Equipments.Dtos;

namespace Synapse.Application.Equipments.Queries;

/// <summary>
/// 設備一覧を取得する。
/// processId を指定すると特定工程の設備のみ返す（工程実績入力画面での絞り込み用途）。
/// </summary>
public record GetEquipmentListQuery(
    Guid? ProcessId = null,
    bool ActiveOnly = true
) : IRequest<List<EquipmentDto>>;

public class GetEquipmentListQueryHandler : IRequestHandler<GetEquipmentListQuery, List<EquipmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEquipmentListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EquipmentDto>> Handle(GetEquipmentListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Equipments
            // 工程名を表示するために Process を JOIN する
            .Include(e => e.Process)
            .AsQueryable();

        // 工程実績入力などの選択画面では廃棄済み設備を表示しないのがデフォルト
        if (request.ActiveOnly)
            query = query.Where(e => e.IsActive);

        // 工程ごとに設備を絞り込む（工程進捗画面・工程実績入力で使用）
        if (request.ProcessId.HasValue)
            query = query.Where(e => e.ProcessId == request.ProcessId.Value);

        // 設備コード順で表示するのが業務上の慣習
        return await query
            .OrderBy(e => e.Code)
            .Select(e => new EquipmentDto(
                e.Id, e.Code, e.Name,
                e.ProcessId, e.Process.Code, e.Process.Name,
                e.IsActive, e.CreatedAt, e.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
