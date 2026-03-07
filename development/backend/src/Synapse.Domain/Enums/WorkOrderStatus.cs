namespace Synapse.Domain.Enums;

/// <summary>
/// 製造指示のステータス。
/// ステータス遷移ルール（詳細: requirements/03_機能要件/03-01_製造指示管理.md MO-007）：
///   Issued → InProgress（着手操作）→ Completed（完了操作）
///   Issued / InProgress → Cancelled（キャンセル操作）
/// 完了・キャンセル後は変更不可。
/// </summary>
public enum WorkOrderStatus
{
    /// <summary>発行済（未着手）。現場が着手前の状態。変更・キャンセル可。</summary>
    Issued = 1,

    /// <summary>着手中。現場が作業を開始した状態。数量変更不可。</summary>
    InProgress = 2,

    /// <summary>完了。全工程が完了した状態。変更不可。</summary>
    Completed = 3,

    /// <summary>キャンセル。発行済または着手中からキャンセルされた状態。変更不可。</summary>
    Cancelled = 4,
}
