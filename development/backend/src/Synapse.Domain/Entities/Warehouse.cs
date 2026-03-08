using Synapse.Domain.Common;
using Synapse.Domain.Enums;

namespace Synapse.Domain.Entities;

/// <summary>
/// 倉庫マスタ。原材料・仕掛品・完成品の保管場所を管理する。
/// 倉庫区分（WarehouseType）により管理対象品目の種別が異なる。
/// </summary>
public class Warehouse : Entity
{
    // EF Core がリフレクションでインスタンス生成するために必要
    private Warehouse() { }

    /// <summary>倉庫コード。システム内で一意な識別子。変更不可。</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>倉庫名。表示・印刷用の名称。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>倉庫区分。格納できる品目の種別を規定する。</summary>
    public WarehouseType WarehouseType { get; private set; }

    /// <summary>有効フラグ。false の場合は廃止済み倉庫（論理削除）。</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 倉庫を新規作成する。初期状態は有効（IsActive = true）。
    /// </summary>
    public static Warehouse Create(string code, string name, WarehouseType warehouseType)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("倉庫コードは必須です。", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("倉庫名は必須です。", nameof(name));

        return new Warehouse
        {
            Code = code,
            Name = name,
            WarehouseType = warehouseType,
            IsActive = true,
        };
    }

    /// <summary>倉庫名・倉庫区分を更新する。</summary>
    public void Update(string name, WarehouseType warehouseType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("倉庫名は必須です。", nameof(name));

        Name = name;
        WarehouseType = warehouseType;
    }

    /// <summary>倉庫を廃止する（論理削除）。</summary>
    public void Deactivate() => IsActive = false;
}
