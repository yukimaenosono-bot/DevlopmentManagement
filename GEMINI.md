# プロジェクトのルールとコンテキスト: Synapse (製造管理システム)

## プロジェクト概要
製造業向けの生産管理システム（MES）。React 19 フロントエンド、.NET 10 API バックエンド、PostgreSQL 16 データベースで構成されています。

## 技術スタック
- **フロントエンド:** React 19 (TypeScript), Vite, TanStack Query, Orval (Zod).
- **バックエンド:** .NET 10, C#, ASP.NET Core API (Clean Architecture).
- **データベース:** PostgreSQL 16.
- **インフラ:** ローカル開発用に Docker Compose (`Makefile`) を使用。

## API-First 開発フロー（黄金サイクル）
API の仕様変更時は、以下の手順を**一連のタスク**として厳守してください。

1.  **バックエンド修正**: DTO や Controller を修正。
2.  **OpenAPI 更新**: `cd development/backend && dotnet build` を実行して `development/openapi.json` を更新。
3.  **クライアント生成**: `cd development/frontend && pnpm generate` を実行。
4.  **フロントエンド追従**: 生成された型や Zod スキーマに基づき、実装を修正。

## 開発・運用ルール
- **Docker 管理:** すべての操作に `Makefile` (`make up`, `make down`, `make logs`) を使用してください。
- **自動生成ファイルの保護:** `src/generated/` 以下のファイルは直接編集せず、必ず `pnpm generate` で更新してください。
- **共通リクエスト設定:** API の共通設定（認証等）は `src/services/apiClient.ts` に集約してください。
- **テスト:** 新規 Command/Query には必ず xUnit テストを追加し、フロントエンド修正後は `pnpm build` で型チェックを行ってください。
