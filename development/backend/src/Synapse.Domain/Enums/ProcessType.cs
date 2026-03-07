namespace Synapse.Domain.Enums;

/// <summary>
/// 工程区分。製造ラインにおける工程の種類を分類する。
/// 区分ごとに使用する設備・品質基準・作業手順が異なる。
/// </summary>
public enum ProcessType
{
    /// <summary>加工工程。素材を切削・成形・溶接等の加工を行う工程。</summary>
    Machining = 1,

    /// <summary>組立工程。複数の部品を結合して製品・中間品を組み立てる工程。</summary>
    Assembly = 2,

    /// <summary>検査工程。寸法・外観・機能の品質確認を行う工程。検査基準マスタと連携する。</summary>
    Inspection = 3,

    /// <summary>包装工程。出荷前の梱包・ラベル貼付を行う工程。</summary>
    Packaging = 4,
}
