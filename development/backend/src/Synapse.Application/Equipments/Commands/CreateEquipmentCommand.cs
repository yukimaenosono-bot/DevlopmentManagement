using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Equipments.Commands;

/// <summary>設備を新規登録するコマンド。戻り値は生成された設備の ID。</summary>
public record CreateEquipmentCommand(
    string Code,
    string Name,
    Guid ProcessId
) : IRequest<Guid>;

public class CreateEquipmentCommandHandler : IRequestHandler<CreateEquipmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateEquipmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {
        // 設備コードは工程実績で参照される識別子のため、重複登録を防ぐ
        var codeExists = await _context.Equipments
            .AnyAsync(e => e.Code == request.Code, cancellationToken);

        if (codeExists)
            throw new InvalidOperationException($"設備コード '{request.Code}' は既に使用されています。");

        // 存在しない工程に設備を登録しようとした場合はエラーにする
        var processExists = await _context.Processes
            .AnyAsync(p => p.Id == request.ProcessId, cancellationToken);

        if (!processExists)
            throw new NotFoundException(nameof(Process), request.ProcessId);

        var equipment = Equipment.Create(request.Code, request.Name, request.ProcessId);

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync(cancellationToken);

        return equipment.Id;
    }
}
