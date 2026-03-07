using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Items.Commands;

/// <summary>品目を論理削除する（IsActive = false にする）。</summary>
public record DeleteItemCommand(Guid Id) : IRequest;

public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Item), request.Id);

        item.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
