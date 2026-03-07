using Synapse.Domain.Common;

namespace Synapse.Domain.Entities;

/// <summary>
/// 設備マスタ。工程内で使用する機械・設備を定義する。
/// 工程実績（t_process_results）から稼働状況の記録先として参照される。
/// 設備は必ず1つの工程に所属する（例: 旋盤A → 旋盤加工工程）。
/// </summary>
public class Equipment : Entity
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private Equipment() { }

    /// <summary>設備コード。社内で一意の識別子（例: "EQ-0001"）。一度登録したら変更不可。</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>設備名。作業指示書・稼働実績画面に表示される名称（例: "旋盤A", "外観検査台1"）。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 所属工程の ID。設備はいずれか1つの工程に割り当てられる。
    /// 工程コード変更不可ルールと同様、一度設定した所属工程の変更は業務上想定しない。
    /// </summary>
    public Guid ProcessId { get; private set; }

    /// <summary>工程ナビゲーションプロパティ。EF Core がリレーションを解決する。</summary>
    public Process Process { get; private set; } = null!;

    /// <summary>
    /// 有効フラグ。false にすると設備は「廃棄・撤去」扱いとなり、新規の工程実績入力では選択不可になる。
    /// 過去の稼働実績に記録が残るよう、物理削除ではなく論理削除を採用している。
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 新規設備を生成する。登録直後は有効（IsActive=true）で始まる。
    /// Code の一意性チェックは Application 層（CreateEquipmentCommandHandler）で行う。
    /// ProcessId の存在チェックも Application 層で行う。
    /// </summary>
    public static Equipment Create(string code, string name, Guid processId)
    {
        return new Equipment
        {
            Code = code,
            Name = name,
            ProcessId = processId,
            IsActive = true,
        };
    }

    /// <summary>
    /// 設備情報を更新する。Code は業務上変更不可のため更新対象から除外している。
    /// </summary>
    public void Update(string name, Guid processId, bool isActive)
    {
        Name = name;
        ProcessId = processId;
        IsActive = isActive;
    }

    /// <summary>
    /// 設備を廃棄・撤去扱いにする（論理削除）。
    /// 過去の工程実績への参照を保持するため物理削除はしない。
    /// </summary>
    public void Deactivate() => IsActive = false;
}
