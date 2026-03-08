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
    DbSet<Warehouse> Warehouses { get; }
    DbSet<Stock> Stocks { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<Routing> Routings { get; }
    DbSet<RoutingStep> RoutingSteps { get; }
    DbSet<WorkOrderOperation> WorkOrderOperations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
