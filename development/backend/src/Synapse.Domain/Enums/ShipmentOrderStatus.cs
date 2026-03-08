namespace Synapse.Domain.Enums;

/// <summary>出荷指示ステータス。</summary>
public enum ShipmentOrderStatus
{
    Draft     = 1,
    Confirmed = 2,
    Picked    = 3,
    Shipped   = 4,
    Cancelled = 5,
}
