using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 出荷指示。受注情報をもとに完成品の出荷を管理するトランザクションエンティティ。
/// 出荷番号: SH-YYYYMMDD-NNNN
/// ステータス: 未確定(Draft) → 出荷待ち(Confirmed) → ピッキング済(Picked) → 出荷済(Shipped)
/// </summary>
public class ShipmentOrder : Entity
{
    private ShipmentOrder() { }

    /// <summary>出荷指示番号。採番ルール: SH-YYYYMMDD-NNNN。</summary>
    public string ShipmentNumber { get; private set; } = string.Empty;

    /// <summary>受注番号（外部受注システム連携）。</summary>
    public string OrderReference { get; private set; } = string.Empty;

    /// <summary>出荷先顧客 ID。</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>顧客ナビゲーションプロパティ。</summary>
    public Customer Customer { get; private set; } = null!;

    /// <summary>出荷品目 ID。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>出荷予定数量。</summary>
    public decimal PlannedQuantity { get; private set; }

    /// <summary>出荷予定日。</summary>
    public DateOnly PlannedShipDate { get; private set; }

    /// <summary>出荷ロット番号（在庫引当時に確定）。</summary>
    public string? LotNumber { get; private set; }

    /// <summary>配送業者名。</summary>
    public string? Carrier { get; private set; }

    /// <summary>特記事項（梱包指定・配送メモ等）。</summary>
    public string? Notes { get; private set; }

    /// <summary>ステータス。</summary>
    public ShipmentOrderStatus Status { get; private set; }

    /// <summary>実出荷日時（UTC）。</summary>
    public DateTime? ShippedAt { get; private set; }

    /// <summary>実出荷数量。</summary>
    public decimal? ActualQuantity { get; private set; }

    /// <summary>出荷担当者ユーザー ID。</summary>
    public string? ShippedByUserId { get; private set; }

    public static ShipmentOrder Create(
        string shipmentNumber,
        string orderReference,
        Guid customerId,
        Guid itemId,
        decimal plannedQuantity,
        DateOnly plannedShipDate,
        string? lotNumber,
        string? carrier,
        string? notes)
    {
        if (plannedQuantity <= 0)
            throw new ArgumentException("出荷予定数量は0より大きい値を指定してください。", nameof(plannedQuantity));

        return new ShipmentOrder
        {
            ShipmentNumber  = shipmentNumber,
            OrderReference  = orderReference,
            CustomerId      = customerId,
            ItemId          = itemId,
            PlannedQuantity = plannedQuantity,
            PlannedShipDate = plannedShipDate,
            LotNumber       = lotNumber,
            Carrier         = carrier,
            Notes           = notes,
            Status          = ShipmentOrderStatus.Draft,
        };
    }

    /// <summary>出荷指示を確定する（出荷待ち）。仮計画のみ確定可能。</summary>
    public void Confirm()
    {
        if (Status != ShipmentOrderStatus.Draft)
            throw new InvalidOperationException("未確定の出荷指示のみ確定できます。");

        Status = ShipmentOrderStatus.Confirmed;
    }

    /// <summary>ピッキング完了に移行する（SH-003）。確定済みのみ可能。</summary>
    public void MarkAsPicked()
    {
        if (Status != ShipmentOrderStatus.Confirmed)
            throw new InvalidOperationException("確定済みの出荷指示のみピッキング完了にできます。");

        Status = ShipmentOrderStatus.Picked;
    }

    /// <summary>出荷実績を登録し出荷済みに移行する（SH-004）。ピッキング済みのみ可能。</summary>
    public void Ship(decimal actualQuantity, string? lotNumber, DateTime shippedAt, string shippedByUserId)
    {
        if (Status != ShipmentOrderStatus.Picked)
            throw new InvalidOperationException("ピッキング済みの出荷指示のみ出荷実績を登録できます。");

        if (actualQuantity <= 0)
            throw new ArgumentException("実出荷数量は0より大きい値を指定してください。", nameof(actualQuantity));

        Status          = ShipmentOrderStatus.Shipped;
        ActualQuantity  = actualQuantity;
        LotNumber       = lotNumber ?? LotNumber;
        ShippedAt       = shippedAt;
        ShippedByUserId = shippedByUserId;
    }

    /// <summary>出荷指示をキャンセルする。出荷済みはキャンセル不可。</summary>
    public void Cancel()
    {
        if (Status == ShipmentOrderStatus.Shipped)
            throw new InvalidOperationException("出荷済みの指示はキャンセルできません。");

        if (Status == ShipmentOrderStatus.Cancelled)
            throw new InvalidOperationException("既にキャンセル済みです。");

        Status = ShipmentOrderStatus.Cancelled;
    }
}
