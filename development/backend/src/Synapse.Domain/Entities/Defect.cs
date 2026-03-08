using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 不良記録。不良発生の記録・原因・対策・処置区分を管理する（QC-004, QC-005）。
/// 不良番号: DF-YYYYMMDD-NNNN
/// </summary>
public class Defect : Entity
{
    private Defect() { }

    /// <summary>不良番号。採番ルール: DF-YYYYMMDD-NNNN。</summary>
    public string DefectNumber { get; private set; } = string.Empty;

    /// <summary>紐付く品質検査 ID（検査記録から発生した場合）。</summary>
    public Guid? QualityInspectionId { get; private set; }

    /// <summary>品質検査ナビゲーションプロパティ。</summary>
    public QualityInspection? QualityInspection { get; private set; }

    /// <summary>紐付く製造指示 ID。</summary>
    public Guid? WorkOrderId { get; private set; }

    /// <summary>製造指示ナビゲーションプロパティ。</summary>
    public WorkOrder? WorkOrder { get; private set; }

    /// <summary>不良品目 ID。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>不良発生日時（UTC）。</summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>不良発生工程 ID。</summary>
    public Guid? ProcessId { get; private set; }

    /// <summary>工程ナビゲーションプロパティ。</summary>
    public Process? Process { get; private set; }

    /// <summary>不良区分（外観・寸法・機能・その他）。</summary>
    public DefectCategory Category { get; private set; }

    /// <summary>不良内容の説明。</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>不良数量。</summary>
    public decimal Quantity { get; private set; }

    /// <summary>推定原因。</summary>
    public string? EstimatedCause { get; private set; }

    /// <summary>是正処置（再発防止策）。</summary>
    public string? CorrectiveAction { get; private set; }

    /// <summary>不良品処置区分。</summary>
    public DispositionType Disposition { get; private set; }

    /// <summary>処置備考。</summary>
    public string? DispositionNote { get; private set; }

    public static Defect Create(
        string defectNumber,
        Guid? qualityInspectionId,
        Guid? workOrderId,
        Guid itemId,
        DateTime occurredAt,
        Guid? processId,
        DefectCategory category,
        string description,
        decimal quantity,
        string? estimatedCause,
        string? correctiveAction,
        DispositionType disposition,
        string? dispositionNote)
    {
        if (quantity <= 0)
            throw new ArgumentException("不良数量は0より大きい値を指定してください。", nameof(quantity));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("不良内容の説明は必須です。", nameof(description));

        return new Defect
        {
            DefectNumber        = defectNumber,
            QualityInspectionId = qualityInspectionId,
            WorkOrderId         = workOrderId,
            ItemId              = itemId,
            OccurredAt          = occurredAt,
            ProcessId           = processId,
            Category            = category,
            Description         = description,
            Quantity            = quantity,
            EstimatedCause      = estimatedCause,
            CorrectiveAction    = correctiveAction,
            Disposition         = disposition,
            DispositionNote     = dispositionNote,
        };
    }

    /// <summary>不良品処置を更新する（QC-005）。</summary>
    public void UpdateDisposition(
        DispositionType disposition,
        string? dispositionNote,
        string? estimatedCause,
        string? correctiveAction)
    {
        Disposition      = disposition;
        DispositionNote  = dispositionNote;
        EstimatedCause   = estimatedCause;
        CorrectiveAction = correctiveAction;
    }
}
