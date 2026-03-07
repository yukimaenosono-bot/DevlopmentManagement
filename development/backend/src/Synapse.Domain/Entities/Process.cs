using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 工程マスタ。製造ラインを構成する個々の作業工程を定義する。
/// 工程ルーティング・工程実績・検査基準マスタから参照される。
/// 品目ごとの製造手順（ルーティング）は、この工程マスタをリンクして構成する。
/// </summary>
public class Process : Entity
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private Process() { }

    /// <summary>工程コード。社内で一意の識別子（例: "PROC-0001"）。一度登録したら変更不可。</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>工程名。作業指示書・進捗画面に表示される名称（例: "旋盤加工", "外観検査"）。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 工程区分。加工・組立・検査・包装で管理方法が異なる。
    /// 特に検査工程は検査基準マスタ（QC）との連携が必要になる。
    /// </summary>
    public ProcessType ProcessType { get; private set; }

    /// <summary>
    /// 有効フラグ。false にすると工程は「廃止」扱いとなり、新規のルーティング設定では選択不可になる。
    /// 過去の工程実績・ルーティングを残すため、物理削除ではなく論理削除を採用している。
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 新規工程を生成する。登録直後は有効（IsActive=true）で始まる。
    /// Code の一意性チェックは Application 層（CreateProcessCommandHandler）で行う。
    /// </summary>
    public static Process Create(string code, string name, ProcessType processType)
    {
        return new Process
        {
            Code = code,
            Name = name,
            ProcessType = processType,
            IsActive = true,
        };
    }

    /// <summary>
    /// 工程情報を更新する。Code は業務上変更不可のため更新対象から除外している。
    /// 工程区分の変更は既存のルーティング・検査基準との整合性に影響するため注意が必要。
    /// </summary>
    public void Update(string name, ProcessType processType, bool isActive)
    {
        Name = name;
        ProcessType = processType;
        IsActive = isActive;
    }

    /// <summary>
    /// 工程を廃止にする（論理削除）。
    /// 過去の工程実績・ルーティングに記録が残るよう、物理削除はしない。
    /// 廃止後は新規ルーティングへの追加で選択できなくなる。
    /// </summary>
    public void Deactivate() => IsActive = false;
}
