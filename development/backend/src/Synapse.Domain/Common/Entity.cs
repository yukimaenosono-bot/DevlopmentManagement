namespace Synapse.Domain.Common;

/// <summary>
/// 全エンティティの基底クラス。IDと更新日時を持つ。
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
