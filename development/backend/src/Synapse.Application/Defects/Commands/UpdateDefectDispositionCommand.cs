using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Enums;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Defects.Commands;

/// <summary>不良品処置を更新する（QC-005）。</summary>
public record UpdateDefectDispositionCommand(
    Guid Id,
    DispositionType Disposition,
    string? DispositionNote,
    string? EstimatedCause,
    string? CorrectiveAction
) : IRequest;

public class UpdateDefectDispositionCommandHandler : IRequestHandler<UpdateDefectDispositionCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDefectDispositionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDefectDispositionCommand request, CancellationToken cancellationToken)
    {
        var defect = await _context.Defects
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Defect), request.Id);

        defect.UpdateDisposition(
            request.Disposition,
            request.DispositionNote,
            request.EstimatedCause,
            request.CorrectiveAction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
