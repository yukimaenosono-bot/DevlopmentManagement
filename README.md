# Synapse (製造管理システム)

次世代の製造現場を支える、高機能な生産管理システム（MES）。
要件定義から詳細設計、クリーンアーキテクチャによる実装までを一貫して管理しています。

## 📂 ディレクトリ構成

- `requirements/` : 要件定義（業務フロー、機能・非機能要件）
- `basic-design/` : 基本設計（画面遷移、ER図、インフラ構成）
- `detailed-design/` : 詳細設計（API仕様、テスト方針）
- `development/` : 実装コード
    - `backend/` : .NET 10 (C#) / Clean Architecture
    - `frontend/` : React 19 / TypeScript / TanStack Query
- `tools/` : 開発支援ツール

## 🚀 クイックスタート

前提: Docker, Node.js 22, .NET SDK 10

```bash
# 1. 全サービスの起動 (DB, API, Frontend)
make up

# 2. フロントエンドの開発サーバ起動 (HMR有効)
cd development/frontend
pnpm install
pnpm dev
```

## 🔄 API 連携フロー (黄金サイクル)

本プロジェクトでは **API-First 開発** を徹底しています。仕様変更時は必ず以下の手順を踏んでください。

1. **Backend**: コントローラ/DTO を修正し `dotnet build`（`openapi.json` が更新される）
2. **Frontend**: `pnpm generate` を実行（Hooks, 型, Zod, MSW が自動更新される）
3. **Refactor**: TypeScript の型エラーに従ってフロントエンドを修正

## 🛠 技術スタック

- **Backend**: .NET 10, Entity Framework Core, MediatR, PostgreSQL
- **Frontend**: React 19, Vite, TanStack Query, Orval, Zod, MSW
- **Infra**: Docker Compose, GCP (Cloud Run, Cloud SQL)

---
詳細は各ディレクトリの README および `CLAUDE.md`, `GEMINI.md` を参照してください。
