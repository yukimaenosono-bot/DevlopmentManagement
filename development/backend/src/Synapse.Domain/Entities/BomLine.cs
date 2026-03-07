namespace Synapse.Domain.Entities;

/// <summary>
/// BOM（部品表）の1行。親品目を製造するために必要な子品目・数量を定義する。
/// 例: 製品A(親) を作るには 部品B(子) が 2個(数量) 必要。
/// 複合PK: (ParentItemId, ChildItemId)。同じ親子ペアを複数登録することはできない。
/// 有効開始日・終了日により、時期によって使用部品が変わる場合（設計変更）に対応する。
/// </summary>
public class BomLine
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private BomLine() { }

    /// <summary>親品目の ID（製造される品目）。Product または Part であることが多い。</summary>
    public Guid ParentItemId { get; private set; }

    /// <summary>親品目ナビゲーションプロパティ。</summary>
    public Item ParentItem { get; private set; } = null!;

    /// <summary>子品目の ID（使用される原材料・部品）。</summary>
    public Guid ChildItemId { get; private set; }

    /// <summary>子品目ナビゲーションプロパティ。</summary>
    public Item ChildItem { get; private set; } = null!;

    /// <summary>
    /// 必要数量。親品目1単位を製造するのに必要な子品目の数量。
    /// 在庫引当・発注量計算の基準値として使用される。
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>数量の単位。子品目マスタの単位と一致させること。</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>
    /// BOM 有効開始日。設計変更時にこの日付を更新することで切り替えを管理する。
    /// この日付以降の製造指示に対してこの BOM 行が適用される。
    /// </summary>
    public DateOnly ValidFrom { get; private set; }

    /// <summary>
    /// BOM 有効終了日。null の場合は無期限有効。
    /// 設計変更で廃止する際に終了日を設定する。
    /// </summary>
    public DateOnly? ValidTo { get; private set; }

    /// <summary>
    /// BOM ラインを生成する。
    /// 親品目と子品目が同一の場合は ArgumentException をスローする（循環参照防止）。
    /// </summary>
    public static BomLine Create(
        Guid parentItemId,
        Guid childItemId,
        decimal quantity,
        string unit,
        DateOnly validFrom,
        DateOnly? validTo)
    {
        if (parentItemId == childItemId)
            throw new ArgumentException("親品目と子品目に同じ品目を設定することはできません。");

        if (quantity <= 0)
            throw new ArgumentException("必要数量は0より大きい値を指定してください。", nameof(quantity));

        if (validTo.HasValue && validTo.Value < validFrom)
            throw new ArgumentException("有効終了日は有効開始日以降を指定してください。", nameof(validTo));

        return new BomLine
        {
            ParentItemId = parentItemId,
            ChildItemId = childItemId,
            Quantity = quantity,
            Unit = unit,
            ValidFrom = validFrom,
            ValidTo = validTo,
        };
    }

    /// <summary>数量・単位・有効期間を更新する。</summary>
    public void Update(decimal quantity, string unit, DateOnly validFrom, DateOnly? validTo)
    {
        if (quantity <= 0)
            throw new ArgumentException("必要数量は0より大きい値を指定してください。");

        if (validTo.HasValue && validTo.Value < validFrom)
            throw new ArgumentException("有効終了日は有効開始日以降を指定してください。");

        Quantity = quantity;
        Unit = unit;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }
}
