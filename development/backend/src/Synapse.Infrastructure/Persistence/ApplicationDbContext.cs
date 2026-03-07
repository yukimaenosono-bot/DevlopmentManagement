using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;
using Synapse.Domain.Common;
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
        // 業務エンティティのマッピング設定はここに追加していく
    }
}
