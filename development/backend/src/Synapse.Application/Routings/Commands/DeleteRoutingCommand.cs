using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Routings.Commands;

/// <summary>ルーティングを論理削除（非アクティブ化）する。</summary>
public record DeleteRoutingCommand(Guid Id) : IRequest;

public class DeleteRoutingCommandHandler : IRequestHandler<DeleteRoutingCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoutingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteRoutingCommand request, CancellationToken cancellationToken)
    {
        var routing = await _context.Routings
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Routing), request.Id);

        routing.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
