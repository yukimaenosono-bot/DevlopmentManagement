using MediatR;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Entities;
using Synapse.Domain.Exceptions;

namespace Synapse.Application.Customers.Commands;

/// <summary>顧客情報を更新する。</summary>
public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? Address,
    string? Phone,
    string? Email
) : IRequest;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        customer.Update(request.Name, request.Address, request.Phone, request.Email);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
