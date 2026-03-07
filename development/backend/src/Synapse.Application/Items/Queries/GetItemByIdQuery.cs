using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Items.Dtos;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Items.Queries;

public record GetItemByIdQuery(Guid Id) : IRequest<ItemDto>;

public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemDto>
{
    private readonly IApplicationDbContext _context;

    public GetItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemDto> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Item), request.Id);

        return new ItemDto(
            item.Id, item.Code, item.Name, item.ShortName, item.ItemType,
            item.Unit, item.StandardUnitPrice, item.SafetyStockQuantity,
            item.HasExpirationDate, item.IsLotManaged, item.IsActive,
            item.CreatedAt, item.UpdatedAt);
    }
}
