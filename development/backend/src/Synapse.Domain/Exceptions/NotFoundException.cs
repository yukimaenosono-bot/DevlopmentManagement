namespace Synapse.Domain.Exceptions;

/// <summary>
/// 対象エンティティが見つからない場合にスローするドメイン例外。
/// APIレイヤーで 404 Not Found にマッピングする。
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName}（ID: {key}）が見つかりません。")
    {
    }
}
