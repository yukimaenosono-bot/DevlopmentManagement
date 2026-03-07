using Microsoft.EntityFrameworkCore;
using Synapse.Application.Common.Interfaces;

namespace Synapse.Infrastructure.Persistence;

/// <summary>
/// EF CoreのDbContext実装。
/// IApplicationDbContextを実装することでApplication層から使えるようにする。
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // エンティティのマッピング設定はここに追加していく
        base.OnModelCreating(modelBuilder);
    }
}
