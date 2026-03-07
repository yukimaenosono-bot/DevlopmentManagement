using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Equipments.Commands;

/// <summary>設備を廃棄・撤去扱いにするコマンド（論理削除）。</summary>
public record DeleteEquipmentCommand(Guid Id) : IRequest;

public class DeleteEquipmentCommandHandler : IRequestHandler<DeleteEquipmentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteEquipmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipments
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Equipment), request.Id);

        // 論理削除。過去の工程実績への参照を保持するため物理削除はしない。
        equipment.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
