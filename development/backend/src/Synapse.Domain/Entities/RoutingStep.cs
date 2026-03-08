using Synapse.Domain.Common;

namespace Synapse.Domain.Entities;

/// <summary>
/// ルーティングの1工程分の定義。
/// Sequence で工程の実施順序を管理する（10, 20, 30... のように10刻みを推奨）。
/// </summary>
public class RoutingStep : Entity
{
    private RoutingStep() { }

    /// <summary>所属するルーティングの ID。</summary>
    public Guid RoutingId { get; private set; }

    /// <summary>工程順序（昇順で実施する）。</summary>
    public int Sequence { get; private set; }

    /// <summary>対象工程の ID。</summary>
    public Guid ProcessId { get; private set; }

    /// <summary>工程ナビゲーションプロパティ。</summary>
    public Process Process { get; private set; } = null!;

    /// <summary>使用設備の ID（任意）。</summary>
    public Guid? EquipmentId { get; private set; }

    /// <summary>設備ナビゲーションプロパティ。</summary>
    public Equipment? Equipment { get; private set; }

    /// <summary>標準作業時間（分）。負荷計算・スケジューリングで使用する。</summary>
    public decimal? StandardTime { get; private set; }

    /// <summary>必須工程フラグ。false の場合はスキップ可能。</summary>
    public bool IsRequired { get; private set; }

    public static RoutingStep Create(
        Guid routingId,
        int sequence,
        Guid processId,
        Guid? equipmentId,
        decimal? standardTime,
        bool isRequired)
    {
        if (sequence <= 0)
            throw new ArgumentException("工程順序は1以上を指定してください。", nameof(sequence));

        return new RoutingStep
        {
            RoutingId    = routingId,
            Sequence     = sequence,
            ProcessId    = processId,
            EquipmentId  = equipmentId,
            StandardTime = standardTime,
            IsRequired   = isRequired,
        };
    }

    public void Update(Guid processId, Guid? equipmentId, decimal? standardTime, bool isRequired)
    {
        ProcessId    = processId;
        EquipmentId  = equipmentId;
        StandardTime = standardTime;
        IsRequired   = isRequired;
    }
}
