using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 生産計画。受注情報・需要予測をもとに立案し、製造指示へ展開するトランザクションエンティティ。
/// 仮計画（Draft）→ 確定（Confirmed）→ 展開済（InProgress）の順に進む。
/// 確定後のみ製造指示への展開が可能（PP-004）。
/// </summary>
public class ProductionPlan : Entity
{
    private ProductionPlan() { }

    /// <summary>計画番号。採番ルール: PP-YYYYMM-NNNN（例: PP-202603-0001）。</summary>
    public string PlanNumber { get; private set; } = string.Empty;

    /// <summary>対象品目の ID。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>計画生産数量。</summary>
    public decimal PlannedQuantity { get; private set; }

    /// <summary>計画開始日。</summary>
    public DateOnly PlanStartDate { get; private set; }

    /// <summary>計画完了日。</summary>
    public DateOnly PlanEndDate { get; private set; }

    /// <summary>完了希望日（納期）。</summary>
    public DateOnly DueDate { get; private set; }

    /// <summary>ステータス。</summary>
    public ProductionPlanStatus Status { get; private set; }

    /// <summary>優先度。</summary>
    public PlanPriority Priority { get; private set; }

    /// <summary>備考。</summary>
    public string? Notes { get; private set; }

    /// <summary>受注番号（外部受注システム連携用）。</summary>
    public string? OrderReference { get; private set; }

    /// <summary>作成者ユーザー ID。</summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    public static ProductionPlan Create(
        string planNumber,
        Guid itemId,
        decimal plannedQuantity,
        DateOnly planStartDate,
        DateOnly planEndDate,
        DateOnly dueDate,
        PlanPriority priority,
        string? notes,
        string? orderReference,
        string createdByUserId)
    {
        if (plannedQuantity <= 0)
            throw new ArgumentException("計画数量は0より大きい値を指定してください。", nameof(plannedQuantity));

        if (planEndDate < planStartDate)
            throw new ArgumentException("計画完了日は開始日以降を指定してください。", nameof(planEndDate));

        return new ProductionPlan
        {
            PlanNumber       = planNumber,
            ItemId           = itemId,
            PlannedQuantity  = plannedQuantity,
            PlanStartDate    = planStartDate,
            PlanEndDate      = planEndDate,
            DueDate          = dueDate,
            Status           = ProductionPlanStatus.Draft,
            Priority         = priority,
            Notes            = notes,
            OrderReference   = orderReference,
            CreatedByUserId  = createdByUserId,
        };
    }

    /// <summary>計画の数量・日程・優先度を更新する。仮計画のみ変更可能。</summary>
    public void Update(
        decimal plannedQuantity,
        DateOnly planStartDate,
        DateOnly planEndDate,
        DateOnly dueDate,
        PlanPriority priority,
        string? notes,
        string? orderReference)
    {
        if (Status != ProductionPlanStatus.Draft)
            throw new InvalidOperationException("仮計画のみ変更できます。");

        if (plannedQuantity <= 0)
            throw new ArgumentException("計画数量は0より大きい値を指定してください。", nameof(plannedQuantity));

        if (planEndDate < planStartDate)
            throw new ArgumentException("計画完了日は開始日以降を指定してください。", nameof(planEndDate));

        PlannedQuantity = plannedQuantity;
        PlanStartDate   = planStartDate;
        PlanEndDate     = planEndDate;
        DueDate         = dueDate;
        Priority        = priority;
        Notes           = notes;
        OrderReference  = orderReference;
    }

    /// <summary>仮計画を確定する（PP-001）。確定後は製造指示展開が可能になる。</summary>
    public void Confirm()
    {
        if (Status != ProductionPlanStatus.Draft)
            throw new InvalidOperationException("仮計画のみ確定できます。");

        Status = ProductionPlanStatus.Confirmed;
    }

    /// <summary>製造指示展開済みに移行する（PP-004 内部処理）。確定済みのみ可能。</summary>
    public void MarkAsInProgress()
    {
        if (Status != ProductionPlanStatus.Confirmed)
            throw new InvalidOperationException("確定済みの計画のみ製造指示に展開できます。");

        Status = ProductionPlanStatus.InProgress;
    }

    /// <summary>計画をキャンセルする。完了済みはキャンセル不可。</summary>
    public void Cancel()
    {
        if (Status == ProductionPlanStatus.Completed)
            throw new InvalidOperationException("完了済みの計画はキャンセルできません。");

        if (Status == ProductionPlanStatus.Cancelled)
            throw new InvalidOperationException("既にキャンセル済みです。");

        Status = ProductionPlanStatus.Cancelled;
    }
}
