using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Processes.Commands;

/// <summary>工程を廃止にするコマンド（論理削除）。</summary>
public record DeleteProcessCommand(Guid Id) : IRequest;

public class DeleteProcessCommandHandler : IRequestHandler<DeleteProcessCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProcessCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProcessCommand request, CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Process), request.Id);

        // 論理削除。過去の工程実績・ルーティングへの参照を保持するため物理削除はしない。
        process.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
