using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 品目マスタ。製品・原材料・部品の基本情報を保持する。
/// BOM・在庫・製造指示・出荷など、ほぼ全業務テーブルから参照される中核マスタ。
/// </summary>
public class Item : Entity
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private Item() { }

    /// <summary>品目コード。社内で一意の識別子（例: "RAW-0001"）。一度登録したら変更不可。</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>品目名。正式名称（伝票・帳票に印字される）。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>品目名略称。画面の一覧表示など、スペースが限られる箇所で使用する。省略可。</summary>
    public string? ShortName { get; private set; }

    /// <summary>品目区分。製品・原材料・部品で在庫の扱いや製造の流れが異なる。</summary>
    public ItemType ItemType { get; private set; }

    /// <summary>在庫・発注・製造指示の数量単位（例: "個", "kg", "m"）。</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>標準単価。原価計算・見積の基準値として使用する。</summary>
    public decimal StandardUnitPrice { get; private set; }

    /// <summary>
    /// 安全在庫数。この数量を下回ると在庫アラート（INV-005）が発報される。
    /// 0 の場合はアラート対象外。
    /// </summary>
    public decimal SafetyStockQuantity { get; private set; }

    /// <summary>
    /// 有効期限管理フラグ。true の場合、入庫時に有効期限の入力が必須となり、
    /// 期限切れアラート（INV-007）の対象になる。医薬品・食品原材料等で使用。
    /// </summary>
    public bool HasExpirationDate { get; private set; }

    /// <summary>
    /// ロット管理フラグ。true の場合、入出庫・製造指示でロット番号の追跡が必須となる。
    /// トレーサビリティ（原材料→製品→出荷先）を実現するために必要。
    /// </summary>
    public bool IsLotManaged { get; private set; }

    /// <summary>
    /// 有効フラグ。false にすると「廃番」扱いとなり、新規の製造指示・発注では選択不可になる。
    /// 過去の実績データを残すために物理削除ではなく論理削除を採用している。
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 新規品目を生成する。登録直後は有効（IsActive=true）で始まる。
    /// Code の一意性チェックは Application 層（CreateItemCommandHandler）で行う。
    /// </summary>
    public static Item Create(
        string code,
        string name,
        string? shortName,
        ItemType itemType,
        string unit,
        decimal standardUnitPrice,
        decimal safetyStockQuantity,
        bool hasExpirationDate,
        bool isLotManaged)
    {
        return new Item
        {
            Code = code,
            Name = name,
            ShortName = shortName,
            ItemType = itemType,
            Unit = unit,
            StandardUnitPrice = standardUnitPrice,
            SafetyStockQuantity = safetyStockQuantity,
            HasExpirationDate = hasExpirationDate,
            IsLotManaged = isLotManaged,
            IsActive = true,
        };
    }

    /// <summary>
    /// 品目情報を更新する。Code は業務上変更不可のため更新対象から除外している。
    /// ロット管理・有効期限管理の変更は在庫の扱いにも影響するため、注意が必要。
    /// </summary>
    public void Update(
        string name,
        string? shortName,
        ItemType itemType,
        string unit,
        decimal standardUnitPrice,
        decimal safetyStockQuantity,
        bool hasExpirationDate,
        bool isLotManaged,
        bool isActive)
    {
        Name = name;
        ShortName = shortName;
        ItemType = itemType;
        Unit = unit;
        StandardUnitPrice = standardUnitPrice;
        SafetyStockQuantity = safetyStockQuantity;
        HasExpirationDate = hasExpirationDate;
        IsLotManaged = isLotManaged;
        IsActive = isActive;
    }

    /// <summary>
    /// 品目を廃番にする（論理削除）。
    /// 過去の製造実績・在庫履歴に品目名が残るよう、物理削除はしない。
    /// 廃番後は製造指示・発注の新規作成で選択できなくなる。
    /// </summary>
    public void Deactivate() => IsActive = false;
}
