using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Processes.Commands;

/// <summary>
/// 工程情報を更新するコマンド。
/// Code（工程コード）は業務上変更不可のため更新対象に含めない。
/// Code を変えたい場合は廃止にして新規登録する運用とする。
/// </summary>
public record UpdateProcessCommand(
    Guid Id,
    string Name,
    ProcessType ProcessType,
    bool IsActive
) : IRequest;

public class UpdateProcessCommandHandler : IRequestHandler<UpdateProcessCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProcessCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProcessCommand request, CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Process), request.Id);

        process.Update(request.Name, request.ProcessType, request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
