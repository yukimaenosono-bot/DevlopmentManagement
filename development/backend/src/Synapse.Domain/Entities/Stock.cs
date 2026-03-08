using Synapse.Domain.Common;

namespace Synapse.Domain.Entities;

/// <summary>
/// 在庫行。品目×倉庫×ロット番号の組み合わせで現在の在庫数量を管理する。
/// 入出庫コマンドにより数量が増減する。数量は常に 0 以上を保証する。
/// </summary>
public class Stock : Entity
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private Stock() { }

    /// <summary>在庫品目の ID。品目マスタへの FK。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>保管倉庫の ID。倉庫マスタへの FK。</summary>
    public Guid WarehouseId { get; private set; }

    /// <summary>倉庫ナビゲーションプロパティ。</summary>
    public Warehouse Warehouse { get; private set; } = null!;

    /// <summary>
    /// ロット番号。ロット管理品目の場合に設定する。
    /// ロット管理なし品目は null。
    /// </summary>
    public string? LotNumber { get; private set; }

    /// <summary>現在在庫数量。入庫で増加、出庫で減少する。</summary>
    public decimal Quantity { get; private set; }

    /// <summary>最終入出庫日時。最後に在庫が動いた日時。</summary>
    public DateTime LastTransactedAt { get; private set; }

    /// <summary>
    /// 在庫行を新規作成する。初期数量は 0。
    /// </summary>
    public static Stock Create(Guid itemId, Guid warehouseId, string? lotNumber)
    {
        return new Stock
        {
            ItemId = itemId,
            WarehouseId = warehouseId,
            LotNumber = lotNumber,
            Quantity = 0,
            LastTransactedAt = DateTime.UtcNow,
        };
    }

    /// <summary>入庫により在庫を増加させる。</summary>
    public void Receive(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("入庫数量は0より大きい値を指定してください。", nameof(quantity));

        Quantity += quantity;
        LastTransactedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 出庫により在庫を減少させる。在庫不足の場合は InvalidOperationException をスローする。
    /// </summary>
    public void Issue(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("出庫数量は0より大きい値を指定してください。", nameof(quantity));

        if (Quantity < quantity)
            throw new InvalidOperationException(
                $"在庫数量が不足しています。現在在庫: {Quantity}、出庫要求: {quantity}");

        Quantity -= quantity;
        LastTransactedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 棚卸調整により在庫数量を指定値に設定する。
    /// </summary>
    public void Adjust(decimal newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentException("調整後在庫数量は0以上の値を指定してください。", nameof(newQuantity));

        Quantity = newQuantity;
        LastTransactedAt = DateTime.UtcNow;
    }
}
