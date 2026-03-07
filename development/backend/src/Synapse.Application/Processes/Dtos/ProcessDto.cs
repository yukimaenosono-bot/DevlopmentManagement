using Synapse.Domain.Enums;

namespace Synapse.Application.Processes.Dtos;

/// <summary>
/// 工程マスタの参照用 DTO。一覧・詳細の両方で使用する。
/// フロントエンドの型定義（ProcessDto）と 1:1 で対応させること。
/// </summary>
public record ProcessDto(
    Guid Id,
    string Code,
    string Name,
    ProcessType ProcessType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
