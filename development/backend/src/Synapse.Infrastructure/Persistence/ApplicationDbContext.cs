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
    }
}
