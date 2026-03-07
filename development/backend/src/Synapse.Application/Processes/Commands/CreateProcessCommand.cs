using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;

namespace Synapse.Application.Processes.Commands;

/// <summary>工程を新規登録するコマンド。戻り値は生成された工程の ID。</summary>
public record CreateProcessCommand(
    string Code,
    string Name,
    ProcessType ProcessType
) : IRequest<Guid>;

public class CreateProcessCommandHandler : IRequestHandler<CreateProcessCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProcessCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProcessCommand request, CancellationToken cancellationToken)
    {
        // 工程コードはルーティング・工程実績で参照される識別子のため、重複登録を防ぐ。
        // DB にもユニーク制約があるが、先に業務例外として返すことでエラーメッセージをわかりやすくする。
        var codeExists = await _context.Processes
            .AnyAsync(p => p.Code == request.Code, cancellationToken);

        if (codeExists)
            throw new InvalidOperationException($"工程コード '{request.Code}' は既に使用されています。");

        var process = Process.Create(request.Code, request.Name, request.ProcessType);

        _context.Processes.Add(process);
        await _context.SaveChangesAsync(cancellationToken);

        return process.Id;
    }
}
