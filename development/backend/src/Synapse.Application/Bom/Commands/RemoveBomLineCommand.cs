using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Bom.Commands;

/// <summary>BOM から子品目を削除するコマンド。</summary>
public record RemoveBomLineCommand(Guid ParentItemId, Guid ChildItemId) : IRequest;

public class RemoveBomLineCommandHandler : IRequestHandler<RemoveBomLineCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveBomLineCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveBomLineCommand request, CancellationToken cancellationToken)
    {
        var bomLine = await _context.BomLines
            .FirstOrDefaultAsync(b =>
                b.ParentItemId == request.ParentItemId &&
                b.ChildItemId == request.ChildItemId, cancellationToken)
            ?? throw new NotFoundException("BomLine",
                $"{request.ParentItemId}/{request.ChildItemId}");

        _context.BomLines.Remove(bomLine);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
