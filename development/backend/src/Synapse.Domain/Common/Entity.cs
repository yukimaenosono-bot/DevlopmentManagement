namespace Synapse.Domain.Common;

/// <summary>
/// 全エンティティの基底クラス。IDと作成日時・更新日時を持つ。
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// DbContext.SaveChangesAsync から呼ばれ、更新日時を現在時刻にセットする。
    /// エンティティ自身から呼ぶ必要はない。
    /// </summary>
    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
