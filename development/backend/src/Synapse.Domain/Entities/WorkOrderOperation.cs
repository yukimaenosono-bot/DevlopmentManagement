using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 製造指示に紐付く工程実績。
/// 製造指示発行時にルーティングから生成され、現場での着手・完了を記録する。
/// WO-001〜007 の工程管理機能の中核エンティティ。
/// </summary>
public class WorkOrderOperation : Entity
{
    private WorkOrderOperation() { }

    /// <summary>対象製造指示の ID。</summary>
    public Guid WorkOrderId { get; private set; }

    /// <summary>製造指示ナビゲーションプロパティ。</summary>
    public WorkOrder WorkOrder { get; private set; } = null!;

    /// <summary>工程順序（ルーティングの Sequence と対応）。</summary>
    public int Sequence { get; private set; }

    /// <summary>対象工程の ID。</summary>
    public Guid ProcessId { get; private set; }

    /// <summary>工程ナビゲーションプロパティ。</summary>
    public Process Process { get; private set; } = null!;

    /// <summary>使用設備の ID（任意）。</summary>
    public Guid? EquipmentId { get; private set; }

    /// <summary>設備ナビゲーションプロパティ。</summary>
    public Equipment? Equipment { get; private set; }

    /// <summary>工程実績ステータス。</summary>
    public WorkOrderOperationStatus Status { get; private set; }

    /// <summary>実際の着手日時（UTC）。</summary>
    public DateTime? ActualStartAt { get; private set; }

    /// <summary>実際の完了日時（UTC）。</summary>
    public DateTime? ActualEndAt { get; private set; }

    /// <summary>完了数量。</summary>
    public decimal? ActualQuantity { get; private set; }

    /// <summary>不良数量。品質管理（QC-004）との連携で使用する。</summary>
    public decimal? DefectQuantity { get; private set; }

    /// <summary>作業者のユーザー ID（ASP.NET Identity の AppUser.Id）。</summary>
    public string? WorkerUserId { get; private set; }

    /// <summary>作業メモ・特記事項。</summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// 工程実績を生成する。製造指示発行時（GenerateOperationsCommand）から呼び出す。
    /// 初期ステータスは Pending（未着手）。
    /// </summary>
    public static WorkOrderOperation Create(
        Guid workOrderId,
        int sequence,
        Guid processId,
        Guid? equipmentId)
    {
        return new WorkOrderOperation
        {
            WorkOrderId = workOrderId,
            Sequence    = sequence,
            ProcessId   = processId,
            EquipmentId = equipmentId,
            Status      = WorkOrderOperationStatus.Pending,
        };
    }

    /// <summary>工程を着手中に移行する（WO-001）。未着手・保留からのみ可能。</summary>
    public void Start(string workerUserId, DateTime startAt)
    {
        if (Status != WorkOrderOperationStatus.Pending && Status != WorkOrderOperationStatus.OnHold)
            throw new InvalidOperationException(
                "未着手または保留中の工程のみ着手できます。");

        Status       = WorkOrderOperationStatus.InProgress;
        WorkerUserId = workerUserId;
        ActualStartAt = ActualStartAt ?? startAt; // 保留からの再開時は着手日時を保持
    }

    /// <summary>工程を完了に移行する（WO-002）。着手中からのみ可能。</summary>
    public void Complete(
        decimal actualQuantity,
        decimal defectQuantity,
        DateTime endAt,
        string? notes)
    {
        if (Status != WorkOrderOperationStatus.InProgress)
            throw new InvalidOperationException("着手中の工程のみ完了にできます。");

        if (actualQuantity < 0)
            throw new ArgumentException("実績数量は0以上を指定してください。", nameof(actualQuantity));

        if (defectQuantity < 0)
            throw new ArgumentException("不良数量は0以上を指定してください。", nameof(defectQuantity));

        Status         = WorkOrderOperationStatus.Completed;
        ActualQuantity = actualQuantity;
        DefectQuantity = defectQuantity;
        ActualEndAt    = endAt;
        Notes          = notes;
    }

    /// <summary>工程を保留にする（WO-002）。着手中からのみ可能。</summary>
    public void PutOnHold()
    {
        if (Status != WorkOrderOperationStatus.InProgress)
            throw new InvalidOperationException("着手中の工程のみ保留にできます。");

        Status = WorkOrderOperationStatus.OnHold;
    }
}
