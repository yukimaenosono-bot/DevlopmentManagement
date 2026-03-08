using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 品質検査記録。受入検査（QC-001）・工程内検査（QC-002）・完成品検査（QC-003）を統一的に管理する。
/// 検査番号: QI-YYYYMMDD-NNNN
/// </summary>
public class QualityInspection : Entity
{
    private QualityInspection() { }

    /// <summary>検査番号。採番ルール: QI-YYYYMMDD-NNNN。</summary>
    public string InspectionNumber { get; private set; } = string.Empty;

    /// <summary>検査種別（受入・工程内・完成品）。</summary>
    public InspectionType InspectionType { get; private set; }

    /// <summary>検査対象品目 ID。</summary>
    public Guid ItemId { get; private set; }

    /// <summary>品目ナビゲーションプロパティ。</summary>
    public Item Item { get; private set; } = null!;

    /// <summary>検査対象ロット番号。</summary>
    public string LotNumber { get; private set; } = string.Empty;

    /// <summary>紐付く製造指示 ID。工程内・完成品検査で設定する。</summary>
    public Guid? WorkOrderId { get; private set; }

    /// <summary>製造指示ナビゲーションプロパティ。</summary>
    public WorkOrder? WorkOrder { get; private set; }

    /// <summary>検査実施日時（UTC）。</summary>
    public DateTime InspectedAt { get; private set; }

    /// <summary>検査員ユーザー ID。</summary>
    public string InspectorUserId { get; private set; } = string.Empty;

    /// <summary>検査数量。</summary>
    public decimal InspectionQuantity { get; private set; }

    /// <summary>合格数量。</summary>
    public decimal PassQuantity { get; private set; }

    /// <summary>不合格数量。</summary>
    public decimal FailQuantity { get; private set; }

    /// <summary>判定結果（合格・不合格・条件付合格）。</summary>
    public InspectionResult Result { get; private set; }

    /// <summary>備考・特記事項。</summary>
    public string? Notes { get; private set; }

    public static QualityInspection Create(
        string inspectionNumber,
        InspectionType inspectionType,
        Guid itemId,
        string lotNumber,
        Guid? workOrderId,
        DateTime inspectedAt,
        string inspectorUserId,
        decimal inspectionQuantity,
        decimal passQuantity,
        decimal failQuantity,
        InspectionResult result,
        string? notes)
    {
        if (inspectionQuantity <= 0)
            throw new ArgumentException("検査数量は0より大きい値を指定してください。", nameof(inspectionQuantity));

        if (passQuantity < 0 || failQuantity < 0)
            throw new ArgumentException("合格数量・不合格数量は0以上を指定してください。");

        if (passQuantity + failQuantity > inspectionQuantity)
            throw new ArgumentException("合格数量と不合格数量の合計が検査数量を超えています。");

        return new QualityInspection
        {
            InspectionNumber   = inspectionNumber,
            InspectionType     = inspectionType,
            ItemId             = itemId,
            LotNumber          = lotNumber,
            WorkOrderId        = workOrderId,
            InspectedAt        = inspectedAt,
            InspectorUserId    = inspectorUserId,
            InspectionQuantity = inspectionQuantity,
            PassQuantity       = passQuantity,
            FailQuantity       = failQuantity,
            Result             = result,
            Notes              = notes,
        };
    }
}
