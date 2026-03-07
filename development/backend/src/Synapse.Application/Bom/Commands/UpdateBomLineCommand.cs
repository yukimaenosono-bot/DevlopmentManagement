using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Bom.Commands;

/// <summary>BOM ラインの数量・単位・有効期間を更新するコマンド。</summary>
public record UpdateBomLineCommand(
    Guid ParentItemId,
    Guid ChildItemId,
    decimal Quantity,
    string Unit,
    DateOnly ValidFrom,
    DateOnly? ValidTo
) : IRequest;

public class UpdateBomLineCommandHandler : IRequestHandler<UpdateBomLineCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateBomLineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateBomLineCommand request, CancellationToken cancellationToken)
    {
        var bomLine = await _context.BomLines
            .FirstOrDefaultAsync(b =>
                b.ParentItemId == request.ParentItemId &&
                b.ChildItemId == request.ChildItemId, cancellationToken)
            ?? throw new NotFoundException("BomLine",
                $"{request.ParentItemId}/{request.ChildItemId}");

        bomLine.Update(request.Quantity, request.Unit, request.ValidFrom, request.ValidTo);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
