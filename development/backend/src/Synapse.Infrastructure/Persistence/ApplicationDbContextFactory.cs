using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Synapse.Infrastructure.Persistence;

/// <summary>
/// dotnet ef migrations add を実行するときにEF CLIがDbContextを作るために使うファクトリ。
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=manufacturing_db;Username=postgres;Password=localpassword")
            .Options;

        return new ApplicationDbContext(options);
    }
}
