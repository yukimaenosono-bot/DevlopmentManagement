namespace Synapse.Application.Customers.Dtos;

public record CustomerDto(
    Guid Id,
    string Code,
    string Name,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
