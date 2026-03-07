using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Equipments.Commands;

/// <summary>
/// 設備情報を更新するコマンド。
/// Code（設備コード）は業務上変更不可のため更新対象に含めない。
/// </summary>
public record UpdateEquipmentCommand(
    Guid Id,
    string Name,
    Guid ProcessId,
    bool IsActive
) : IRequest;

public class UpdateEquipmentCommandHandler : IRequestHandler<UpdateEquipmentCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateEquipmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipments
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Equipment), request.Id);

        // 変更先の工程が存在するか確認する
        var processExists = await _context.Processes
            .AnyAsync(p => p.Id == request.ProcessId, cancellationToken);

        if (!processExists)
            throw new NotFoundException(nameof(Process), request.ProcessId);

        equipment.Update(request.Name, request.ProcessId, request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
