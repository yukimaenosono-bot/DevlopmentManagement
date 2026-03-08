namespace Synapse.Domain.Enums;

/// <summary>工程実績のステータス。</summary>
public enum WorkOrderOperationStatus
{
    Pending    = 1, // 未着手
    InProgress = 2, // 着手中
    Completed  = 3, // 完了
    OnHold     = 4, // 保留
}
