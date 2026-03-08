/**
 * API レスポンスで使用するプリミティブ型エイリアス。
 * orval が生成する型は string を使うが、ユーティリティ関数や
 * フォームスキーマの引数型に使用することで意図を明示する。
 */

/** ULID 形式の ID 文字列（例: "01ARZ3NDEKTSV4RRFFQ69G5FAV"） */
export type Ulid = string

/** UTC ISO 8601 タイムスタンプ文字列（例: "2026-03-07T10:00:00Z"）。C# の DateTimeOffset に対応 */
export type IsoDateTimeString = string

/** ISO 8601 日付文字列（例: "2026-03-07"）。C# の DateOnly に対応 */
export type IsoDateString = string
