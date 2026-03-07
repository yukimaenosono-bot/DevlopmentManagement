using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrders.Commands;

/// <summary>
/// 製造指示を着手中に移行するコマンド（MO-007）。
/// 発行済（Issued）状態からのみ移行可能。
/// 着手後は指示数量が変更不可になる（工程実績との整合性を保つため）。
/// </summary>
public record StartWorkOrderCommand(Guid Id) : IRequest;

public class StartWorkOrderCommandHandler : IRequestHandler<StartWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public StartWorkOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StartWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        // 遷移可否チェックはエンティティ側で担保する（Issued 以外は例外）
        workOrder.Start();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
