using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrders.Commands;

/// <summary>
/// 製造指示を完了に移行するコマンド（MO-007）。
/// 着手中（InProgress）状態からのみ移行可能。
/// 完了後は変更・キャンセル不可になる。工程実績・品質実績が確定するタイミング。
/// </summary>
public record CompleteWorkOrderCommand(Guid Id) : IRequest;

public class CompleteWorkOrderCommandHandler : IRequestHandler<CompleteWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteWorkOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        // 遷移可否チェックはエンティティ側で担保する（InProgress 以外は例外）
        workOrder.Complete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
