using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Items.Commands;

/// <summary>
/// 品目情報を更新するコマンド。
/// Code（品目コード）は業務上変更不可のため更新対象に含めない。
/// Code を変えたい場合は廃番にして新規登録する運用とする。
/// </summary>
public record UpdateItemCommand(
    Guid Id,
    string Name,
    string? ShortName,
    ItemType ItemType,
    string Unit,
    decimal StandardUnitPrice,
    decimal SafetyStockQuantity,
    bool HasExpirationDate,
    bool IsLotManaged,
    bool IsActive
) : IRequest;

public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Item), request.Id);

        item.Update(
            request.Name,
            request.ShortName,
            request.ItemType,
            request.Unit,
            request.StandardUnitPrice,
            request.SafetyStockQuantity,
            request.HasExpirationDate,
            request.IsLotManaged,
            request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
