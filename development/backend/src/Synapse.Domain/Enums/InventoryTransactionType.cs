namespace Synapse.Domain.Enums;

/// <summary>
/// 入出庫区分。在庫の増減原因を分類する。
/// 履歴照会や原価計算でフィルタリングに使用する。
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>購買入庫。仕入先からの原材料受入。関連伝票: 発注番号。</summary>
    PurchaseReceipt = 1,

    /// <summary>製造完成入庫。製造指示の完了に伴う完成品入庫。関連伝票: 製造指示番号。</summary>
    ManufacturingReceipt = 2,

    /// <summary>返品入庫。出庫済み品の返品。</summary>
    ReturnReceipt = 3,

    /// <summary>出庫。原材料の製造払出・完成品の出荷。関連伝票: 製造指示番号 or 出荷指示番号。</summary>
    Issue = 4,

    /// <summary>棚卸調整。帳簿在庫と実地在庫の差異を調整する。</summary>
    Adjustment = 5,

    /// <summary>その他入庫。上記区分に該当しない入庫。</summary>
    OtherReceipt = 6,
}
