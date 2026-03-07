using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 製造指示。現場に対して「いつ・何を・いくつ作るか」を指示するトランザクションエンティティ。
/// 生産計画（ProductionPlan）から生成される場合と、手動発行の場合がある。
/// 工程実績（t_process_results）・品質実績（t_quality_results）が紐付く中核エンティティ。
/// </summary>
public class WorkOrder : Entity
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private WorkOrder() { }

    /// <summary>
    /// 製造指示番号。採番ルール: MO-YYYYMMDD-NNNN（例: MO-20260307-0001）。
    /// 発行日＋当日連番で一意性を保証する。変更不可。
    /// </summary>
    public string WorkOrderNumber { get; private set; } = string.Empty;

    /// <summary>
    /// 生産計画番号。計画から自動生成された場合に設定される。
    /// 手動発行の場合は null。現時点では参照整合性なしの文字列として保持（計画マスタ未実装）。
    /// </summary>
    public string? ProductionPlanNumber { get; private set; }

    /// <summary>製造対象品目の ID。品目マスタへの FK。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。EF Core がリレーションを解決する。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>
    /// 製造ロット番号。ロット管理品目の場合は必須。
    /// トレーサビリティ（原材料→製品→出荷先）のキーとなる。
    /// </summary>
    public string? LotNumber { get; private set; }

    /// <summary>指示数量。着手中になると変更不可（工程実績と齟齬が生じるため）。</summary>
    public decimal Quantity { get; private set; }

    /// <summary>製造開始予定日。工程スケジューリングの基準日。</summary>
    public DateOnly PlannedStartDate { get; private set; }

    /// <summary>完了予定日。遅延アラート（WO-006）の判定基準日。</summary>
    public DateOnly PlannedEndDate { get; private set; }

    /// <summary>
    /// ステータス。発行済→着手中→完了 の順に進む。
    /// 完了・キャンセル後は変更不可（工程実績・品質実績が確定しているため）。
    /// </summary>
    public WorkOrderStatus Status { get; private set; }

    /// <summary>作業指示コメント。現場への特記事項・特別指示を記載する。</summary>
    public string? WorkInstruction { get; private set; }

    /// <summary>発行者のユーザー ID（ASP.NET Identity の AppUser.Id）。</summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>
    /// 新規製造指示を生成する。初期ステータスは Issued（発行済・未着手）。
    /// WorkOrderNumber の採番は Application 層（CreateWorkOrderCommandHandler）で行う。
    /// </summary>
    public static WorkOrder Create(
        string workOrderNumber,
        string? productionPlanNumber,
        Guid itemId,
        string? lotNumber,
        decimal quantity,
        DateOnly plannedStartDate,
        DateOnly plannedEndDate,
        string? workInstruction,
        string createdByUserId)
    {
        if (quantity <= 0)
            throw new ArgumentException("指示数量は0より大きい値を指定してください。", nameof(quantity));

        if (plannedEndDate < plannedStartDate)
            throw new ArgumentException("完了予定日は開始予定日以降を指定してください。", nameof(plannedEndDate));

        return new WorkOrder
        {
            WorkOrderNumber = workOrderNumber,
            ProductionPlanNumber = productionPlanNumber,
            ItemId = itemId,
            LotNumber = lotNumber,
            Quantity = quantity,
            PlannedStartDate = plannedStartDate,
            PlannedEndDate = plannedEndDate,
            Status = WorkOrderStatus.Issued,
            WorkInstruction = workInstruction,
            CreatedByUserId = createdByUserId,
        };
    }

    /// <summary>
    /// 製造指示を更新する。ステータスに応じた変更可否チェックを行う。
    /// 完了・キャンセル済みは変更不可。着手中は数量変更不可。
    /// </summary>
    public void Update(
        decimal quantity,
        DateOnly plannedStartDate,
        DateOnly plannedEndDate,
        string? workInstruction)
    {
        if (Status == WorkOrderStatus.Completed || Status == WorkOrderStatus.Cancelled)
            throw new InvalidOperationException(
                $"ステータスが「{Status}」の製造指示は変更できません。");

        if (Status == WorkOrderStatus.InProgress && quantity != Quantity)
            throw new InvalidOperationException(
                "着手中の製造指示の指示数量は変更できません。");

        if (quantity <= 0)
            throw new ArgumentException("指示数量は0より大きい値を指定してください。");

        if (plannedEndDate < plannedStartDate)
            throw new ArgumentException("完了予定日は開始予定日以降を指定してください。");

        Quantity = quantity;
        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        WorkInstruction = workInstruction;
    }

    /// <summary>
    /// 製造指示をキャンセルする。発行済・着手中のみキャンセル可能。
    /// キャンセル済み・完了済みに対してはエラーとする。
    /// </summary>
    public void Cancel()
    {
        if (Status == WorkOrderStatus.Completed)
            throw new InvalidOperationException("完了済みの製造指示はキャンセルできません。");

        if (Status == WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("既にキャンセル済みです。");

        Status = WorkOrderStatus.Cancelled;
    }

    /// <summary>製造指示を着手中に移行する。発行済状態からのみ可能。</summary>
    public void Start()
    {
        if (Status != WorkOrderStatus.Issued)
            throw new InvalidOperationException(
                "発行済（未着手）の製造指示のみ着手できます。");

        Status = WorkOrderStatus.InProgress;
    }

    /// <summary>製造指示を完了に移行する。着手中状態からのみ可能。</summary>
    public void Complete()
    {
        if (Status != WorkOrderStatus.InProgress)
            throw new InvalidOperationException(
                "着手中の製造指示のみ完了にできます。");

        Status = WorkOrderStatus.Completed;
    }
}
