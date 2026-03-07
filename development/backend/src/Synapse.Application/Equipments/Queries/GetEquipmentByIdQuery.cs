using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Equipments.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Equipments.Queries;

/// <summary>設備を1件取得する。存在しない場合は NotFoundException をスローする。</summary>
public record GetEquipmentByIdQuery(Guid Id) : IRequest<EquipmentDto>;

public class GetEquipmentByIdQueryHandler : IRequestHandler<GetEquipmentByIdQuery, EquipmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetEquipmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EquipmentDto> Handle(GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var equipment = await _context.Equipments
            .Include(e => e.Process)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Equipment), request.Id);

        return new EquipmentDto(
            equipment.Id, equipment.Code, equipment.Name,
            equipment.ProcessId, equipment.Process.Code, equipment.Process.Name,
            equipment.IsActive, equipment.CreatedAt, equipment.UpdatedAt);
    }
}
