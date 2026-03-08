using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Common;
using Synapse.Domain.Entities;
using Synapse.Infrastructure.Identity;

namespace Synapse.Infrastructure.Persistence;

/// <summary>
/// EF CoreのDbContext。
/// IdentityDbContext を継承することでユーザー・ロールテーブルが自動生成される。
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AppUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Process> Processes => Set<Process>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Routing> Routings => Set<Routing>();
    public DbSet<RoutingStep> RoutingSteps => Set<RoutingStep>();
    public DbSet<WorkOrderOperation> WorkOrderOperations => Set<WorkOrderOperation>();
    public DbSet<ProductionPlan> ProductionPlans => Set<ProductionPlan>();
    public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
    public DbSet<Defect> Defects => Set<Defect>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ShipmentOrder> ShipmentOrders => Set<ShipmentOrder>();

    /// <summary>
    /// 保存時に Entity.UpdatedAt を自動更新する。
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.Touch();
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Item>(b =>
        {
            b.ToTable("m_items");
            b.HasIndex(i => i.Code).IsUnique();
            b.Property(i => i.Code).HasMaxLength(50);
            b.Property(i => i.Name).HasMaxLength(200);
            b.Property(i => i.ShortName).HasMaxLength(100);
            b.Property(i => i.Unit).HasMaxLength(20);
            b.Property(i => i.StandardUnitPrice).HasPrecision(18, 4);
            b.Property(i => i.SafetyStockQuantity).HasPrecision(18, 4);
        });

        modelBuilder.Entity<Process>(b =>
        {
            b.ToTable("m_processes");
            b.HasIndex(p => p.Code).IsUnique();
            b.Property(p => p.Code).HasMaxLength(50);
            b.Property(p => p.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Equipment>(b =>
        {
            b.ToTable("m_equipments");
            b.HasIndex(e => e.Code).IsUnique();
            b.Property(e => e.Code).HasMaxLength(50);
            b.Property(e => e.Name).HasMaxLength(200);
            // 設備は1つの工程に所属する（工程削除時に設備は残す）
            b.HasOne(e => e.Process)
             .WithMany()
             .HasForeignKey(e => e.ProcessId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkOrder>(b =>
        {
            b.ToTable("t_work_orders");
            // 製造指示番号はユーザーが参照する識別子のため、一意制約を設ける
            b.HasIndex(w => w.WorkOrderNumber).IsUnique();
            b.Property(w => w.WorkOrderNumber).HasMaxLength(30);
            b.Property(w => w.ProductionPlanNumber).HasMaxLength(50);
            b.Property(w => w.LotNumber).HasMaxLength(100);
            b.Property(w => w.Quantity).HasPrecision(18, 4);
            b.Property(w => w.WorkInstruction).HasMaxLength(1000);
            b.Property(w => w.CreatedByUserId).HasMaxLength(450); // Identity の UserId 最大長
            // 品目マスタは参照のみ（品目削除時に製造指示は残す）
            b.HasOne(w => w.Item)
             .WithMany()
             .HasForeignKey(w => w.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BomLine>(b =>
        {
            b.ToTable("m_bom");
            // 複合主キー: 親品目 + 子品目
            b.HasKey(bl => new { bl.ParentItemId, bl.ChildItemId });
            b.Property(bl => bl.Unit).HasMaxLength(20);
            b.Property(bl => bl.Quantity).HasPrecision(18, 4);
            // 親品目・子品目はどちらも削除不可（BOM が参照しているため）
            b.HasOne(bl => bl.ParentItem)
             .WithMany()
             .HasForeignKey(bl => bl.ParentItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(bl => bl.ChildItem)
             .WithMany()
             .HasForeignKey(bl => bl.ChildItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Warehouse>(b =>
        {
            b.ToTable("m_warehouses");
            b.HasIndex(w => w.Code).IsUnique();
            b.Property(w => w.Code).HasMaxLength(50);
            b.Property(w => w.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Stock>(b =>
        {
            b.ToTable("t_inventories");
            b.Property(s => s.LotNumber).HasMaxLength(100);
            b.Property(s => s.Quantity).HasPrecision(18, 4);
            // 在庫照会の主要アクセスパターン: 品目+倉庫+ロット
            b.HasIndex(s => new { s.ItemId, s.WarehouseId, s.LotNumber });
            // 品目・倉庫は削除不可（在庫が参照しているため）
            b.HasOne(s => s.Item)
             .WithMany()
             .HasForeignKey(s => s.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(s => s.Warehouse)
             .WithMany()
             .HasForeignKey(s => s.WarehouseId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransaction>(b =>
        {
            b.ToTable("t_inventory_histories");
            b.Property(t => t.LotNumber).HasMaxLength(100);
            b.Property(t => t.Quantity).HasPrecision(18, 4);
            b.Property(t => t.ReferenceNumber).HasMaxLength(100);
            b.Property(t => t.Note).HasMaxLength(500);
            b.Property(t => t.CreatedByUserId).HasMaxLength(450);
            b.HasIndex(t => t.TransactedAt);
            b.HasOne(t => t.Item)
             .WithMany()
             .HasForeignKey(t => t.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(t => t.Warehouse)
             .WithMany()
             .HasForeignKey(t => t.WarehouseId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Routing>(b =>
        {
            b.ToTable("m_routings");
            b.Property(r => r.Name).HasMaxLength(200);
            // 品目削除不可（ルーティングが参照しているため）
            b.HasOne(r => r.Item)
             .WithMany()
             .HasForeignKey(r => r.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(r => new { r.ItemId, r.IsDefault });
        });

        modelBuilder.Entity<RoutingStep>(b =>
        {
            b.ToTable("m_routing_steps");
            b.Property(s => s.StandardTime).HasPrecision(10, 2);
            // Routing 削除時にステップも一括削除
            b.HasOne<Routing>()
             .WithMany(r => r.Steps)
             .HasForeignKey(s => s.RoutingId)
             .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(s => s.Process)
             .WithMany()
             .HasForeignKey(s => s.ProcessId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(s => s.Equipment)
             .WithMany()
             .HasForeignKey(s => s.EquipmentId)
             .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(s => new { s.RoutingId, s.Sequence }).IsUnique();
        });

        modelBuilder.Entity<WorkOrderOperation>(b =>
        {
            b.ToTable("t_work_order_operations");
            b.Property(o => o.ActualQuantity).HasPrecision(18, 4);
            b.Property(o => o.DefectQuantity).HasPrecision(18, 4);
            b.Property(o => o.WorkerUserId).HasMaxLength(450);
            b.Property(o => o.Notes).HasMaxLength(1000);
            // 製造指示削除不可（実績が参照しているため）
            b.HasOne(o => o.WorkOrder)
             .WithMany()
             .HasForeignKey(o => o.WorkOrderId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(o => o.Process)
             .WithMany()
             .HasForeignKey(o => o.ProcessId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(o => o.Equipment)
             .WithMany()
             .HasForeignKey(o => o.EquipmentId)
             .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(o => o.WorkOrderId);
            b.HasIndex(o => new { o.WorkOrderId, o.Sequence });
        });

        modelBuilder.Entity<ProductionPlan>(b =>
        {
            b.ToTable("t_production_plans");
            b.HasIndex(p => p.PlanNumber).IsUnique();
            b.Property(p => p.PlanNumber).HasMaxLength(20);
            b.Property(p => p.PlannedQuantity).HasPrecision(18, 4);
            b.Property(p => p.Notes).HasMaxLength(1000);
            b.Property(p => p.OrderReference).HasMaxLength(100);
            b.Property(p => p.CreatedByUserId).HasMaxLength(450);
            // 品目削除不可（生産計画が参照しているため）
            b.HasOne(p => p.Item)
             .WithMany()
             .HasForeignKey(p => p.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => new { p.ItemId, p.Status });
        });

        modelBuilder.Entity<QualityInspection>(b =>
        {
            b.ToTable("t_quality_inspections");
            b.HasIndex(q => q.InspectionNumber).IsUnique();
            b.Property(q => q.InspectionNumber).HasMaxLength(20);
            b.Property(q => q.LotNumber).HasMaxLength(100);
            b.Property(q => q.InspectorUserId).HasMaxLength(450);
            b.Property(q => q.InspectionQuantity).HasPrecision(18, 4);
            b.Property(q => q.PassQuantity).HasPrecision(18, 4);
            b.Property(q => q.FailQuantity).HasPrecision(18, 4);
            b.Property(q => q.Notes).HasMaxLength(1000);
            b.HasOne(q => q.Item)
             .WithMany()
             .HasForeignKey(q => q.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(q => q.WorkOrder)
             .WithMany()
             .HasForeignKey(q => q.WorkOrderId)
             .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(q => q.InspectionType);
            b.HasIndex(q => new { q.ItemId, q.InspectedAt });
        });

        modelBuilder.Entity<Defect>(b =>
        {
            b.ToTable("t_defects");
            b.HasIndex(d => d.DefectNumber).IsUnique();
            b.Property(d => d.DefectNumber).HasMaxLength(20);
            b.Property(d => d.Description).HasMaxLength(1000);
            b.Property(d => d.Quantity).HasPrecision(18, 4);
            b.Property(d => d.EstimatedCause).HasMaxLength(1000);
            b.Property(d => d.CorrectiveAction).HasMaxLength(1000);
            b.Property(d => d.DispositionNote).HasMaxLength(500);
            b.HasOne(d => d.Item)
             .WithMany()
             .HasForeignKey(d => d.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(d => d.WorkOrder)
             .WithMany()
             .HasForeignKey(d => d.WorkOrderId)
             .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(d => d.QualityInspection)
             .WithMany()
             .HasForeignKey(d => d.QualityInspectionId)
             .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(d => d.Process)
             .WithMany()
             .HasForeignKey(d => d.ProcessId)
             .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(d => d.ItemId);
            b.HasIndex(d => new { d.ItemId, d.OccurredAt });
        });

        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("m_customers");
            b.HasIndex(c => c.Code).IsUnique();
            b.Property(c => c.Code).HasMaxLength(50);
            b.Property(c => c.Name).HasMaxLength(200);
            b.Property(c => c.Address).HasMaxLength(500);
            b.Property(c => c.Phone).HasMaxLength(50);
            b.Property(c => c.Email).HasMaxLength(200);
        });

        modelBuilder.Entity<ShipmentOrder>(b =>
        {
            b.ToTable("t_shipment_orders");
            b.HasIndex(s => s.ShipmentNumber).IsUnique();
            b.Property(s => s.ShipmentNumber).HasMaxLength(20);
            b.Property(s => s.OrderReference).HasMaxLength(100);
            b.Property(s => s.LotNumber).HasMaxLength(100);
            b.Property(s => s.Carrier).HasMaxLength(200);
            b.Property(s => s.Notes).HasMaxLength(1000);
            b.Property(s => s.PlannedQuantity).HasPrecision(18, 4);
            b.Property(s => s.ActualQuantity).HasPrecision(18, 4);
            b.Property(s => s.ShippedByUserId).HasMaxLength(450);
            b.HasOne(s => s.Customer)
             .WithMany()
             .HasForeignKey(s => s.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(s => s.Item)
             .WithMany()
             .HasForeignKey(s => s.ItemId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(s => s.Status);
            b.HasIndex(s => new { s.CustomerId, s.Status });
            b.HasIndex(s => s.PlannedShipDate);
        });
    }
}
