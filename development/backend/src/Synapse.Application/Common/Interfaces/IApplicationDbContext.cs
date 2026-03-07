namespace Synapse.Application.Common.Interfaces;

/// <summary>
/// Application層からDBにアクセスするためのインターフェース。
/// Infrastructure層の実装に依存しないようにするための抽象化。
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
