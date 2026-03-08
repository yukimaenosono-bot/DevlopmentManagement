namespace Synapse.Domain.Enums;

/// <summary>生産計画のステータス。</summary>
public enum ProductionPlanStatus
{
    Draft      = 1, // 仮計画
    Confirmed  = 2, // 確定
    InProgress = 3, // 製造指示展開済み
    Completed  = 4, // 完了
    Cancelled  = 5, // キャンセル
}
