using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Items.Commands;

/// <summary>品目を新規登録するコマンド。戻り値は生成された品目の ID。</summary>
public record CreateItemCommand(
    string Code,
    string Name,
    string? ShortName,
    ItemType ItemType,
    string Unit,
    decimal StandardUnitPrice,
    decimal SafetyStockQuantity,
    bool HasExpirationDate,
    bool IsLotManaged
) : IRequest<Guid>;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // 品目コードはシステム全体の識別子として使われるため、重複登録を防ぐ。
        // DBにもユニーク制約があるが、先に業務例外として返すことでエラーメッセージをわかりやすくする。
        var codeExists = await _context.Items
            .AnyAsync(i => i.Code == request.Code, cancellationToken);

        if (codeExists)
            throw new InvalidOperationException($"品目コード '{request.Code}' は既に使用されています。");

        var item = Item.Create(
            request.Code,
            request.Name,
            request.ShortName,
            request.ItemType,
            request.Unit,
            request.StandardUnitPrice,
            request.SafetyStockQuantity,
            request.HasExpirationDate,
            request.IsLotManaged);

        _context.Items.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
