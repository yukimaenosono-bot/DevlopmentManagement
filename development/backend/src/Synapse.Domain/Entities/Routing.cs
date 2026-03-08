using Synapse.Domain.Common;

namespace Synapse.Domain.Entities;

/// <summary>
/// 工程ルーティングマスタ。品目に対する標準工程順序を定義する。
/// 製造指示発行時にこのルーティングから WorkOrderOperation（工程実績）が生成される。
/// 1つの品目に対して複数のルーティングを持てるが、IsDefault=true のものが既定として使われる。
/// </summary>
public class Routing : Entity
{
    private Routing() { }

    private readonly List<RoutingStep> _steps = new();

    /// <summary>対象品目の ID。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>ルーティング名（例: "標準工程", "外注工程"）。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>品目のデフォルトルーティングかどうか。</summary>
    public bool IsDefault { get; private set; }

    /// <summary>有効フラグ。</summary>
    public bool IsActive { get; private set; }

    /// <summary>工程ステップ一覧（順序付き）。</summary>
    public IReadOnlyList<RoutingStep> Steps => _steps.AsReadOnly();

    public static Routing Create(Guid itemId, string name, bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ルーティング名を入力してください。", nameof(name));

        return new Routing
        {
            ItemId    = itemId,
            Name      = name,
            IsDefault = isDefault,
            IsActive  = true,
        };
    }

    public void Update(string name, bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("ルーティング名を入力してください。", nameof(name));

        Name      = name;
        IsDefault = isDefault;
    }

    public void Deactivate() => IsActive = false;
}
