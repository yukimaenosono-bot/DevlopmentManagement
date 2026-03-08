using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Application.Customers.Dtos;

namespace Synapse.Application.Customers.Queries;

/// <summary>顧客一覧を取得する。</summary>
public record GetCustomerListQuery(bool ActiveOnly = true) : IRequest<List<CustomerDto>>;

public class GetCustomerListQueryHandler : IRequestHandler<GetCustomerListQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> Handle(GetCustomerListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsQueryable();

        if (request.ActiveOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CustomerDto(
                c.Id, c.Code, c.Name,
                c.Address, c.Phone, c.Email,
                c.IsActive, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
