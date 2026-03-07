using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Bom.Commands;

/// <summary>BOM に子品目を追加するコマンド。</summary>
public record AddBomLineCommand(
    Guid ParentItemId,
    Guid ChildItemId,
    decimal Quantity,
    string Unit,
    DateOnly ValidFrom,
    DateOnly? ValidTo
) : IRequest;

public class AddBomLineCommandHandler : IRequestHandler<AddBomLineCommand>
{
    private readonly IApplicationDbContext _context;

    public AddBomLineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AddBomLineCommand request, CancellationToken cancellationToken)
    {
        // 親品目・子品目の存在確認
        var parentExists = await _context.Items
            .AnyAsync(i => i.Id == request.ParentItemId, cancellationToken);
        if (!parentExists)
            throw new NotFoundException(nameof(Item), request.ParentItemId);

        var childExists = await _context.Items
            .AnyAsync(i => i.Id == request.ChildItemId, cancellationToken);
        if (!childExists)
            throw new NotFoundException(nameof(Item), request.ChildItemId);

        // 同じ親子ペアの重複チェック（DB ユニーク制約の前段確認）
        var alreadyExists = await _context.BomLines
            .AnyAsync(b => b.ParentItemId == request.ParentItemId &&
                           b.ChildItemId == request.ChildItemId, cancellationToken);
        if (alreadyExists)
            throw new InvalidOperationException(
                "この親品目と子品目の組み合わせは既に BOM に登録されています。");

        var bomLine = BomLine.Create(
            request.ParentItemId,
            request.ChildItemId,
            request.Quantity,
            request.Unit,
            request.ValidFrom,
            request.ValidTo);

        _context.BomLines.Add(bomLine);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
