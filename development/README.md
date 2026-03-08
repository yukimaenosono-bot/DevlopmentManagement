# SYNAPSE - 製造管理システム

## 技術スタック

| レイヤー | 技術 |
|---------|------|
| フロントエンド | React 19 + TypeScript (Vite + TanStack Query) |
| バックエンド | ASP.NET Core 10 Web API (C# / Clean Architecture) |
| データベース | PostgreSQL 16 (Docker / Cloud SQL) |
| API 連携 | OpenAPI + Orval (React Query & Zod 自動生成) |

## ローカル起動

### 前提
- Docker Desktop
- Node.js 22 + pnpm
- .NET SDK 10

### 起動

```bash
# 全サービス起動（DB + API + Frontend）
# プロジェクトルートの Makefile を利用可能
make up

# フロントエンド開発サーバ起動
cd frontend && pnpm dev

# バックエンド起動
cd backend/src/Synapse.API && dotnet run
```

---

## 🔄 API 連携 & 保守ワークフロー（黄金サイクル）

API の仕様変更（フィールド追加、型変更、新規エンドポイント作成など）が発生した際は、以下の手順でフロントエンドを同期させます。

### 1. バックエンドの修正とビルド
バックエンドのコントローラや DTO を修正し、ビルドを実行します。
```bash
cd development/backend
dotnet build Synapse.sln
```
*   **ポイント**: ビルドが成功すると、`development/openapi.json` が自動的に更新されます。
*   **注意**: ビルドが失敗する場合、`Synapse.API/appsettings.json` の `Jwt:SecretKey` や `ConnectionStrings` が正しく設定されているか確認してください。

### 2. フロントエンドのコード自動生成
Orval を使用して、API クライアント（Hooks）と Zod スキーマを再生成します。
```bash
cd development/frontend
pnpm generate
```
*   **生成場所**:
    *   `src/generated/`: API 呼び出し用の Hooks (TanStack Query)
    *   `src/generated/model/`: TypeScript の型定義
    *   `src/generated/zod/`: フォームバリデーション用の Zod スキーマ

### 3. フロントエンドの修正（型エラーの解消）
仕様変更によって既存のコードと不整合が起きた場合、TypeScript がコンパイルエラーとして教えてくれます。
```bash
# 型チェックを実行してエラー箇所を特定
pnpm build
```
*   エディタ（VS Code 等）の「問題」タブに表示されるエラーを修正するだけで、安全に仕様変更に追従できます。

---

## 🛠️ 開発時の注意点

### 自動生成ファイルについて
`src/generated/` 以下のファイルは `pnpm generate` によって**上書きされます。直接編集しないでください。**

### 共通リクエスト設定 (Mutator)
認証トークンの付与やベースURLの設定、共通のエラーハンドリングは `src/services/apiClient.ts` で管理しています。
Orval はすべての API リクエストでこの `apiClient` を使用するように設定されています。

### Zod スキーマの活用
自動生成された Zod スキーマ（`src/generated/zod/`）は、React Hook Form 等のバリデーションにそのまま利用できます。
```typescript
import { postApiAuthLoginBody } from '@/generated/zod/auth/auth.zod';
// ... resolver: zodResolver(postApiAuthLoginBody)
```

---

## フォルダ構成

```
development/
├── frontend/          # React + TypeScript (Vite)
│   ├── src/
│   │   ├── generated/ # ★ Orval による自動生成コード
│   │   └── services/  # apiClient.ts (共通リクエスト設定)
├── backend/           # ASP.NET Core 10 (.NET Solution)
│   ├── src/
│   │   ├── Synapse.API/
│   │   ├── Synapse.Application/
│   │   ├── Synapse.Domain/
│   │   └── Synapse.Infrastructure/
│   └── tests/
├── openapi.json       # ★ バックエンドビルド時に更新される API 仕様書
└── docker-compose.yml # ローカル開発環境
```
