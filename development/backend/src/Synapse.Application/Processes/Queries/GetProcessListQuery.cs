using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Processes.Dtos;

namespace Synapse.Application.Processes.Queries;

/// <summary>工程一覧を取得する。IsActive=true のみを返すのがデフォルト。</summary>
public record GetProcessListQuery(bool ActiveOnly = true) : IRequest<List<ProcessDto>>;

public class GetProcessListQueryHandler : IRequestHandler<GetProcessListQuery, List<ProcessDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProcessListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProcessDto>> Handle(GetProcessListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Processes.AsQueryable();

        // ルーティング設定・製造指示の工程選択画面では廃止工程を表示しないのがデフォルト。
        // 廃止工程も含めて参照したい場合（マスタ管理画面等）は activeOnly=false で呼ぶ。
        if (request.ActiveOnly)
            query = query.Where(p => p.IsActive);

        // 工程コード順で表示するのが業務上の慣習
        return await query
            .OrderBy(p => p.Code)
            .Select(p => new ProcessDto(
                p.Id, p.Code, p.Name, p.ProcessType,
                p.IsActive, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
