using MediatR;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;

namespace Synapse.Application.Customers.Commands;

/// <summary>顧客を新規登録する。</summary>
public record CreateCustomerCommand(
    string Code,
    string Name,
    string? Address,
    string? Phone,
    string? Email
) : IRequest<Guid>;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            request.Code, request.Name,
            request.Address, request.Phone, request.Email);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
