using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Processes.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Processes.Queries;

/// <summary>工程を1件取得する。存在しない場合は NotFoundException をスローする。</summary>
public record GetProcessByIdQuery(Guid Id) : IRequest<ProcessDto>;

public class GetProcessByIdQueryHandler : IRequestHandler<GetProcessByIdQuery, ProcessDto>
{
    private readonly IApplicationDbContext _context;

    public GetProcessByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessDto> Handle(GetProcessByIdQuery request, CancellationToken cancellationToken)
    {
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Process), request.Id);

        return new ProcessDto(
            process.Id, process.Code, process.Name, process.ProcessType,
            process.IsActive, process.CreatedAt, process.UpdatedAt);
    }
}
