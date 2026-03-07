using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Entities;

namespace Synapse.Application.Common.Interfaces;

/// <summary>
/// Application層からDBにアクセスするためのインターフェース。
/// Infrastructure層の実装に依存しないようにするための抽象化。
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Item> Items { get; }
    DbSet<Process> Processes { get; }
    DbSet<Equipment> Equipments { get; }
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<BomLine> BomLines { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
