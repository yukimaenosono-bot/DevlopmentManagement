using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 入出庫履歴。在庫の増減事実を記録する不変のトランザクションログ。
/// 在庫照会の監査証跡として保持し、削除・変更は行わない。
/// Adjustment（棚卸調整）では Quantity が負（差分値）になることがある。
/// </summary>
public class InventoryTransaction : Entity
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private InventoryTransaction() { }

    /// <summary>対象品目の ID。品目マスタへの FK。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>対象倉庫の ID。倉庫マスタへの FK。</summary>
    public Guid WarehouseId { get; private set; }

    /// <summary>倉庫ナビゲーションプロパティ。</summary>
    public Warehouse Warehouse { get; private set; } = null!;

    /// <summary>ロット番号。ロット管理品目の場合に設定する。</summary>
    public string? LotNumber { get; private set; }

    /// <summary>入出庫区分。在庫の増減理由を分類する。</summary>
    public InventoryTransactionType TransactionType { get; private set; }

    /// <summary>
    /// 数量。入庫・出庫は正の値。棚卸調整は正負どちらもあり得る（差分値）。
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// 関連伝票番号。入庫の場合は発注番号、出庫の場合は製造指示番号や出荷指示番号。
    /// 任意入力。
    /// </summary>
    public string? ReferenceNumber { get; private set; }

    /// <summary>備考・特記事項。</summary>
    public string? Note { get; private set; }

    /// <summary>処理日時。入出庫操作を行った日時。</summary>
    public DateTime TransactedAt { get; private set; }

    /// <summary>担当者のユーザー ID。</summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    /// 入出庫履歴を記録する。
    /// </summary>
    public static InventoryTransaction Create(
        Guid itemId,
        Guid warehouseId,
        string? lotNumber,
        InventoryTransactionType transactionType,
        decimal quantity,
        string? referenceNumber,
        string? note,
        string createdByUserId)
    {
        return new InventoryTransaction
        {
            ItemId = itemId,
            WarehouseId = warehouseId,
            LotNumber = lotNumber,
            TransactionType = transactionType,
            Quantity = quantity,
            ReferenceNumber = referenceNumber,
            Note = note,
            TransactedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
        };
    }
}
