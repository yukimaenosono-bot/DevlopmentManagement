using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.WorkOrders.Commands;

/// <summary>
/// 製造指示を更新するコマンド。
/// ステータスに応じた変更可否チェックはドメインエンティティ（WorkOrder.Update）で行う。
/// </summary>
public record UpdateWorkOrderCommand(
    Guid Id,
    decimal Quantity,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    string? WorkInstruction
) : IRequest;

public class UpdateWorkOrderCommandHandler : IRequestHandler<UpdateWorkOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        // ステータスチェック・着手中の数量変更禁止はエンティティ側で担保する
        workOrder.Update(
            request.Quantity,
            request.PlannedStartDate,
            request.PlannedEndDate,
            request.WorkInstruction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
