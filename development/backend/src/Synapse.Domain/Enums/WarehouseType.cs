namespace Synapse.Domain.Enums;

/// <summary>
/// 倉庫区分。倉庫の用途・役割を分類する。
/// 区分ごとに格納できる品目区分・棚卸頻度・管理ルールが異なる。
/// </summary>
public enum WarehouseType
{
    /// <summary>原材料倉庫。購買入庫品の保管場所。</summary>
    RawMaterial = 1,

    /// <summary>製造中間倉庫。仕掛品・半製品の一時保管場所。</summary>
    WorkInProgress = 2,

    /// <summary>完成品倉庫。出荷待ち製品の保管場所。</summary>
    FinishedGoods = 3,

    /// <summary>汎用倉庫。区分を問わず保管する倉庫。</summary>
    General = 4,
}
